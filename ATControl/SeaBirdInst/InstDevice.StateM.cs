using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;
using System.Timers;
using NLog;
using Newtonsoft;
using Newtonsoft.Json.Linq;

namespace InstDevice
{
    /// <summary>
    /// 仪器当前的状态
    /// </summary>
    public enum StateInst : int
    {
        /// <summary> 空闲 </summary>
        Idle = 0,
        /// <summary> 测量数据 </summary>
        Measure,
        /// <summary> 存储数据 </summary>
        Store
    }

    /// <summary>
    /// 仪器状态改变的触发事件
    /// </summary>
    internal enum TriggerInst : int
    {
        /// <summary> 时刻 </summary>
        TimerTick,
        /// <summary> 开始测量 </summary>
        StartMeasure,
        /// <summary> 开始存储数据 </summary>
        StartStore,
        /// <summary> 停止 </summary>
        Stop
    }

    /// <summary>
    /// 仪器基类 - 加入状态机 - 主要是实现了不同状态极其执行函数
    /// </summary>
    /// <typeparam name="TInfo"> 仪器状态类 </typeparam>
    /// <typeparam name="TData"> 仪器数据类 </typeparam>
    public abstract partial class InstDeviceStateM<TData, TInfo> : InstDeviceBase
        where TData : InstDataBase, new()
        where TInfo : InstInfoBase, new()
    {
        /// <summary>
        /// 仪器数据缓存
        /// </summary>
        protected List<TData> _instData = new List<TData>();
        /// <summary>
        /// 用于存储的缓存数据
        /// </summary>
        protected List<TData> _storeCache = new List<TData>();
        private int dataMaxLen = 1000;
        /// <summary>
        /// 当前读取到的数据缓存
        /// </summary>
        protected TData instData1Cache = new TData();
        /// <summary>
        /// 设备线程锁，同一时间只允许单一线程访问设备资源（串口 / 数据）
        /// </summary>
        protected object _instLocker = new object();
        /// <summary>
        /// 设备的信息 Info
        /// </summary>
        public TInfo Info { get; set; }
        /// <summary>
        /// 采样时间 - 秒
        /// </summary>
        public static int sampleIntervalSec = 10;



        /// <summary>
        /// 状态机类
        /// </summary>
        private StateMachine<StateInst, TriggerInst> _instMachine;
        /// <summary>
        /// 当前处于的状态
        /// </summary>
        private StateInst _instState { set; get; } = StateInst.Idle;
        /// <summary>
        /// 时刻Trigger - 带参数 ms
        /// </summary>
        private StateMachine<StateInst, TriggerInst>.TriggerWithParameters<int> _instTickTrigger;

        /// <summary>
        /// 定时器
        /// </summary>
        protected Timer _tickTimerInst;
        /// <summary>
        /// 自动取样 - 状态计数器
        /// </summary>
        private uint _instStateCounts = 0;
        /// <summary>
        /// 当前需要测量的温度点
        /// </summary>
        protected double currentTemptPoint = 0.0f;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ifo"> 设备信息 Info </param>
        public InstDeviceStateM(TInfo ifo)
        {
            Info = ifo;
            Info.FreshFromSql2Info();
        }
        /// <summary>
        /// 获取仪器的基本信息
        /// </summary>
        /// <returns></returns>
        public override InstInfoBase GetBasicInfo()
        {
            return Info as InstInfoBase;
        }
        /// <summary>
        /// 初始化仪器设备，通过 Info 信息及 cfg 包含的端口信息
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public override bool InitWithInfo()
        {
            bool confOK = true;
            if (this.InstIdx >= MaxInstNum)
            {
                nlogger.Error("the InstIdx exceed the maxInstNum: " + InstCount.ToString());
                return false;
            }

            // 记录设备的编号
            Info.InstIdx_NotUsed = this.InstIdx;

            if(Info.InstType == TypeInst.Undefined) {
                nlogger.Error("the Info.InstType undefined.");
                return false;
            }

            try
            {
                // 设置波特率
                confOK &= SetPortName(Info.PortName, Info.BaudRate);
            }
            catch (Exception ex)
            {
                nlogger.Error("exception occur when SetPortName: " + ex.Message);
                return false;
            }

            // 配置端口失败
            if(confOK == false)
            {
                nlogger.Error("failed to config the serial port.");
                return false;
            }

            // 使能
            if(confOK == true) { Enable = true; }
            return confOK;
        }
        /// <summary>
        /// 返回当前仪器的值
        /// todo: 这个函数是放在 InstDeviceBase 里面好呢，
        /// 还是放在 InstDeviceStateM 里面好呢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<TData> GetInstData()
        {
            return _instData;
        }


        /// <summary>
        /// 执行读取信息步骤
        /// </summary>
        /// <param name="temptPoint"> 当前温度点 </param>
        /// <returns></returns>
        public override bool StartMeasure(double temptPoint = 0)
        {
            currentTemptPoint = temptPoint;

            if (!Enable) return  true;

            if (_instState != StateInst.Idle)
            {
                return false;
            }

            // 开始测量
            _instMachine.Fire(TriggerInst.StartMeasure);
            return true;
        }
        /// <summary>
        /// 开始存储数据
        /// </summary>
        /// <param name="temptPoint"> 当前温度点 </param>
        /// <returns></returns>
        public override bool StartStore(double temptPoint = 0)
        {
            if (!Enable) return true;

            if (_instState != StateInst.Measure)
            {
                return false;
            }

            nlogger.Warn("step 2 : start store");

            // 停止测量，并进入存储数据
            _instMachine.Fire(TriggerInst.StartStore);
            // 执行一次步骤
            _instMachine.Fire(_instTickTrigger, 10);
            return true;
        }
        /// <summary>
        /// 停止测量数据
        /// </summary>
        /// <param name="temptPoint"> 当前温度点 </param>
        /// <returns></returns>
        public override bool StopMeasure(double temptPoint = 0)
        {
            if (!Enable) return false;

            _instMachine.Fire(TriggerInst.Stop);
            return true;
        }
        /// <summary>
        /// 执行 Idle 步骤
        /// </summary>
        protected virtual void internalIdleStep() { }
        /// <summary>
        /// 进入 Measure 步骤
        /// </summary>
        protected virtual void internalEnterMeasureStep() {
            // 清空数据缓存
            _storeCache.Clear();
        }
        /// <summary>
        /// 执行 Measure 步骤
        /// </summary>
        protected virtual void internalMeasureStep() {

        }
        /// <summary>
        /// 进入 Store 步骤
        /// </summary>
        protected virtual void internalEnterStoreStep() {
            
        }
        /// <summary>
        /// 执行 Store 步骤
        /// </summary>
        protected virtual void internalStoreStep() {
            nlogger.Warn("step 3 : internal store step.");
            // 设备未启用
            if (Enable == false) return;

            // 将数据写入数据库
            if (sqlWriter.InsertValue(_storeCache) == false)
            {
                // 写入数据库失败
                nlogger.Error("Error in InsertValue.");
                OnErrorOccur(Err_sr.Error);
            }
        }

        /// <summary>
        /// 处理串口接收到的数据
        /// </summary>
        /// <param name="str"></param>
        protected override void internalProceedReceivedData(string str)
        {
            // 设备未启用
            if (Enable == false) return;

            // 解析收到的字符串，看是否是指令，如果是指令（cmd）则返回
            // 如果不是，则需要进一步解析
            bool rlt1 = ResolveStr2Cmd(str);
            if(rlt1 == true)
            {
                return;
            }

            // 解析字符串，看是否是结束符号，如果是结束符（Executed）则返回
            // 如果不是，则需要进一步解析
            bool rlt2 = ResolveStr2ExecutedFlag(str);
            if(rlt2 == true)
            {
                return;
            }

            TData dt;
            bool rlt = ResolveStr2Data(str, out dt);
            // 由字符串解析为数据，发生错误
            if (rlt == false)
            {
                nlogger.Error("Error in ResolveData");
                OnErrorOccur(Err_sr.Error);
                return;
            }
            // 由字符串解析为数据，暂时未解析成一组数据（有时可能要两组数据）
            if (dt == null) return;

            // 只有在 Measure 状态，才会存储数据
            if (_instState == StateInst.Measure)
            {
                appendStoreCache(dt);
            }
            // 记录当前数据
            appendInstData(dt);

            // 触发数据接收事件
            OnDataReceived(dt);
        }
        /// <summary>
        /// 由收到的字符串解析为指令。
        /// 其实就是检查 <Executed/>
        /// </summary>
        /// <param name="str"> 串口接收到的字符串 </param>
        /// <returns> 是否成功解析为指令 </returns>
        protected abstract bool ResolveStr2Cmd(string str);
        /// <summary>
        /// 由接收到的字符串解析为结束符 Executed
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected virtual bool ResolveStr2ExecutedFlag(string str) { return false; }
        /// <summary>
        /// 由收到的字符串解析为数据。
        /// 根据不同的传感器/配置，进行数据的解析
        /// </summary>
        /// <param name="str"> 串口接收到的字符串 </param>
        /// <param name="dt"> 解析得到的数据，为空表示 暂时未解析成一组数据 </param>
        /// <returns> 是否成功解析为数据 </returns>
        protected abstract bool ResolveStr2Data(string str, out TData dt);


        /// <summary>
        /// 配置仪器相关的状态机
        /// </summary>
        protected void ConfigInstStateless()
        {
            // new object
            _instMachine = new StateMachine<StateInst, TriggerInst>(() => _instState, s => _instState = s);
            _instTickTrigger = _instMachine.SetTriggerParameters<int>(TriggerInst.TimerTick);

            // on transition action
            _instMachine.OnTransitioned(instOnTransitionedAction);

            // on unhandled trigger
            _instMachine.OnUnhandledTrigger(instOnUnhandledTrigger);

            // StateInst.Idle
            // -> StateInst.Measure
            _instMachine.Configure(StateInst.Idle)
                .OnEntry(t => instIdleEntry())
                .OnExit(t => instIdleExit())
                .InternalTransition(_instTickTrigger, (tic, t) => instIdleTick(tic))
                .Permit(TriggerInst.StartMeasure, StateInst.Measure)
                .Ignore(TriggerInst.StartStore)
                .Ignore(TriggerInst.Stop);


            // StateInst.Measure
            // -> StateInst.StoreData
            // -> StateInst.Idle
            _instMachine.Configure(StateInst.Measure)
                .OnEntry(t => instMeasureEntry())
                .OnExit(t => instMeasureExit())
                .InternalTransition(_instTickTrigger, (tic, t) => instMeasureTick(tic))
                .Permit(TriggerInst.StartStore, StateInst.Store)
                .Permit(TriggerInst.Stop, StateInst.Idle);


            // StateInst.Store
            // -> StateInst.Idle
            _instMachine.Configure(StateInst.Store)
                .OnEntry(t => instStoreEntry())
                .OnExit(t => instStoreExit())
                .InternalTransition(_instTickTrigger, (tic, t) => instStoreTick(tic))
                .Permit(TriggerInst.Stop, StateInst.Idle)
                .Ignore(TriggerInst.StartMeasure);
        }

        /// <summary>
        /// 仪器单步执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void _tickTimerSample_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 计数
            _instStateCounts++;

            _instMachine.Fire(_instTickTrigger, 10);
        }
        /// <summary>
        /// 状态转变事件函数
        /// </summary>
        /// <param name="act"></param>
        private void instOnTransitionedAction(StateMachine<StateInst, TriggerInst>.Transition act)
        {
            nlogger.Debug("On StateInst Transitioned.");

            TriggerInst trigger = act.Trigger;
            StateInst source = act.Source;
            StateInst dest = act.Destination;

            // 状态清零
            _instStateCounts = 0;

            // 触发事件 - 状态转变
            StateChangedEvent?.Invoke(dest);
        }
        /// <summary>
        /// 未定义事件函数
        /// </summary>
        /// <param name="st"></param>
        /// <param name="tg"></param>
        private void instOnUnhandledTrigger(StateInst st, TriggerInst tg)
        {
            nlogger.Error("Inst Unhandled trigger: state.");
        }
        /// <summary>
        /// Inst Idle Entry
        /// </summary>
        protected void instIdleEntry()
        {
            nlogger.Debug("Inst Idle Entry.");
        }
        /// <summary>
        /// Inst Idle Tick
        /// </summary>
        /// <param name="tic"></param>
        protected void instIdleTick(int tic)
        {
            nlogger.Debug("Inst Idle Tick: " + tic.ToString() + " ms");

            // 执行 idle 步骤
            internalIdleStep();
        }
        /// <summary>
        /// Inst Idle Exit
        /// </summary>
        protected void instIdleExit()
        {
            nlogger.Debug("Inst Idle Exit.");
        }
        /// <summary>
        /// Inst Measure Entry
        /// </summary>
        protected void instMeasureEntry()
        {
            nlogger.Debug("Inst Measure Entry.");

            // 进入 store 步骤
            internalEnterMeasureStep();
        }
        /// <summary>
        /// Inst Measure Tick
        /// </summary>
        /// <param name="tic"></param>
        protected void instMeasureTick(int tic)
        {
            nlogger.Debug("Inst Measure Tick: " + tic.ToString() + " ms");

            // 执行 Measure 步骤
            internalMeasureStep();
        }
        /// <summary>
        /// instrInstument Measure Exit
        /// </summary>
        protected void instMeasureExit()
        {
            nlogger.Debug("Inst Measure Exit.");
        }
        /// <summary>
        /// Inst Store Entry
        /// </summary>
        protected void instStoreEntry()
        {
            nlogger.Debug("Inst Store Entry.");

            // 进入 store 步骤
            internalEnterStoreStep();
        }
        /// <summary>
        /// Inst Store Tick
        /// </summary>
        /// <param name="tic"></param>
        protected void instStoreTick(int tic)
        {
            nlogger.Debug("Inst Store Tick: " + tic.ToString() + " ms");

            // 执行 store 步骤
            internalStoreStep();

            // 进入空闲状态
            _instMachine.Fire(TriggerInst.Stop);
        }
        /// <summary>
        /// Inst Store Exit
        /// </summary>
        protected void instStoreExit()
        {
            nlogger.Debug("Inst Store Exit.");
        }

        /// <summary>
        /// 向 _instData 值列表中添加数据
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="val"></param>
        protected void appendInstData(TData val)
        {
            if (_instData.Count == dataMaxLen)
            {
                _instData.RemoveAt(0);
            }
            _instData.Add(val);
        }
        /// <summary>
        /// 向 _storeCache 值列表中添加数据
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="val"></param>
        protected void appendStoreCache(TData val)
        {
            if (_storeCache.Count == dataMaxLen)
            {
                _storeCache.RemoveAt(0);
            }
            _storeCache.Add(val);
        }
    }
}
