using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Diagnostics;

//using System.Windows.Forms;

using System.Runtime.InteropServices;





namespace GoldsrcPhysics

{

    class TimerDate

    {

        public Stopwatch Timer { get; set; }

        public uint UpdateFrames { get; set; }

        public uint RenderFrames { get; set; }

        public int SkipedFrames { get; protected set; }



        //public double Time { get { return timer.Elapsed.TotalSeconds; } }

        public int Second { get { return Timer.Elapsed.Seconds; } }

        protected double oneFrameTime;



        public double OverTime

        {

            get;

            set;

        }

        public double UpdatedTime

        {

            get;

            protected set;

        }

        protected double BeforeUpdateTime

        {

            get;

            set;

        }

        public double RenderedTime

        {

            get;

            protected set;

        }

        protected double BeforeRenderTime

        {

            get;

            set;

        }



        public virtual void TimerInit(int fps)

        {

            Timer = new Stopwatch();

            UpdateFrames = 0;

            RenderFrames = 0;

            SkipedFrames = 0;

            OverTime = 0;

            oneFrameTime = 1.0 / (double)fps;

            UpdatedTime = 0;

            BeforeUpdateTime = 0;

        }



        public void TimerStart()

        {

            if (Timer != null && !Timer.IsRunning)

                Timer.Start();

        }

        public void TimerStop()

        {

            if (Timer != null && Timer.IsRunning)

                Timer.Stop();

        }

    }



    class FPSController : TimerDate

    {

        public bool RenderOK { get; private set; }

        public bool Pausing { get; set; }

        int fps;

        public int Fps

        {

            get

            { return fps; }

            set

            {

                fps = value;

                oneFrameTime = 1.0 / (double)fps;

            }

        }



        public double Interval

        {

            get { return oneFrameTime; }

            set

            {

                oneFrameTime = value;

                fps = (int)(1.0 / oneFrameTime);

            }

        }



        public double WaitTime

        {

            get;

            protected set;

        }



        public override void TimerInit(int fps)

        {

            Pausing = false;

            RenderOK = true;

            WaitTime = 0;

            if (fps < 1)

            {

                base.TimerInit(60);

            }

            base.TimerInit(fps);

        }



        public void BeforeUpdate()

        {

            BeforeUpdateTime = Timer.Elapsed.TotalSeconds;

        }

        public void AfterUpdate()

        {

            UpdatedTime = Timer.Elapsed.TotalSeconds - BeforeUpdateTime;

            UpdateFrames++;

        }

        public void BeforeRender()

        {

            BeforeRenderTime = Timer.Elapsed.TotalSeconds;

        }

        public void AfterRender()

        {

            RenderedTime = Timer.Elapsed.TotalSeconds;

            RenderFrames++;

            SkipedFrames = 0;

        }



        public void JudgeRenderOK()

        {

            WaitTime = oneFrameTime - (Timer.Elapsed.TotalSeconds - BeforeUpdateTime);//処理に掛かった時間から待ち時間を決める

            if (WaitTime <= 0 || !RenderOK)

            {

                OverTime -= WaitTime;

                if (OverTime <= 0)

                {

                    RenderOK = true;

                    WaitTime = -OverTime;

                    OverTime = 0;

                    return;

                }

                RenderOK = false;

                WaitTime = 0;

                SkipedFrames++;

            }



        }

        public void Wait()

        {

            double waitingTimes = WaitTime + Timer.Elapsed.TotalSeconds;

            while (Timer.Elapsed.TotalSeconds < waitingTimes)

                if (Timer.Elapsed.TotalSeconds + 0.001 < waitingTimes) System.Threading.Thread.Sleep(1);

        }

        public virtual void PauseCheck()

        {

            if (Pausing)

            {

                TimerStop();

                GamePause();

                while (Pausing) { System.Threading.Thread.Sleep(100); }

                GameResume();

                TimerStart();

            }

        }

        public event GamingEventHandler GamePause, GameResume;



        public void Pause()

        {

            Pausing = true;

        }

        public void Resume()

        {

            Pausing = false;

        }

    }

    /// <summary>

    /// FPSTimerクラスのイベントを使用するためのデリゲートです

    /// </summary>

    public delegate void GamingEventHandler();



    /// <summary>

    /// FPSを安定させて動作させるためのタイマーです

    /// </summary>

    public class FPSTimerNonThread

    {

        /// <summary>

        /// ゲームループを開始する前に呼び出されます。初期化処理を代入してください

        /// </summary>

        public event GamingEventHandler GameInit;

        /// <summary>

        /// ゲームループを開始する前に呼び出されます。データのロード処理を代入してください

        /// </summary>

        public event GamingEventHandler GameLoad;

        /// <summary>

        /// ゲームループで更新の際に呼び出されます。更新処理を必ず代入してください(描画の前に呼び出されます)

        /// </summary>

        public event GamingEventHandler GameUpdate;

        /// <summary>

        /// ゲームループの描画の際に呼び出されます。描画処理を必ず代入してください(更新の後に呼び出されます)

        /// </summary>

        public event GamingEventHandler GameRender;

        /// <summary>

        /// ゲームループがポーズ状態に入った時に呼び出されます。ポーズメニュー画面の表示等を行なってください(これもタイマー側のスレッドで呼び出されるのでWindowsFormのコントロールを操作する場合はInvoke処理を書いてください)

        /// </summary>

        public event GamingEventHandler GamePause;

        /// <summary>

        /// ポーズ状態を解除した時に呼び出されます。ポーズ画面の非表示化等を行ってください(スレッドに関してはGamePauseと同様)

        /// </summary>

        public event GamingEventHandler GameResume;

        /// <summary>

        /// ゲームループのが終了する際に呼び出されます。後始末等の処理をを代入してください

        /// </summary>

        public event GamingEventHandler GameExit;

        FPSController timer;

        public int Second { get { return timer.Second; } }



        public FPSTimerNonThread()

        {

            timer = new FPSController();

            timer.GamePause += new GamingEventHandler(timer_GamePause);

            timer.GameResume += new GamingEventHandler(timer_GameResume);

            IsRunning = false;

            updateCounter = 0;

            renderCounter = 0;

            measureTime = 0;

        }



        void timer_GameResume()

        {

            if (GameResume != null)

                this.GameResume();

        }



        void timer_GamePause()

        {

            if (GamePause != null)

                this.GamePause();

        }



        #region パブリックプロパティ

        /// <summary>

        /// タイマーが実行中かどうかを取得します

        /// </summary>

        public bool IsRunning { get; protected set; }

        /// <summary>

        /// 前回スキップしたフレーム数を取得します

        /// </summary>

        public int SkipedFrames

        {

            get { return timer.SkipedFrames; }

        }

        /// <summary>

        /// 今まで更新処理をした回数を取得します

        /// </summary>

        public uint UpdateFrames

        {

            get { return timer.UpdateFrames; }

        }

        /// <summary>

        /// 今まで描画処理をした回数を取得します

        /// </summary>

        public uint RenderFrames

        {

            get { return timer.RenderFrames; }

        }

        /// <summary>

        /// 1秒間に何回処理するかを指定、取得します

        /// </summary>

        public int Fps

        {

            get { return timer.Fps; }

            set { timer.Fps = value; }

        }



        /// <summary>

        /// 何秒感覚で作動させるかを指定、取得します

        /// </summary>

        public double Interval

        {

            get { return timer.Interval; }

            set { timer.Interval = value; }

        }



        /// <summary>

        /// 1秒間に何回更新しているかを取得します

        /// </summary>

        public int UpdateFps { get; private set; }

        /// <summary>

        /// 1秒間に何回描画できてるかを取得します

        /// </summary>

        public int RenderFps { get; private set; }

        /// <summary>

        /// タイマーが始まってからの時間を取得します

        /// </summary>

        public double Time

        {

            get { return timer.Timer.Elapsed.TotalSeconds; }

        }

        /// <summary>

        /// タイマーがポーズ状態であるかを取得、設定します

        /// </summary>

        public bool Pausing

        {

            get { return timer.Pausing; }

            set

            {

                if (IsRunning)

                {

                    timer.Pausing = value;

                }

            }

        }



        #endregion パブリックプロパティ



        #region パブリックメソッド

        /// <summary>

        /// タイマーをポーズ状態(一時停止)にします

        /// </summary>

        public void Pause()

        {

            Pausing = true;

        }

        /// <summary>

        /// ポーズ状態のタイマーを再稼働させます

        /// </summary>

        public void Resume()

        {

            Pausing = false;

        }



        /// <summary>

        /// ゲームループを開始します

        /// </summary>

        public virtual void GameStart()

        {

            if (Fps < 1)

            {

                Fps = 60;

            }

            GameMainStart();

        }

        /// <summary>

        /// 引数に指定した数字をFpsに設定し、ゲームループを開始します

        /// </summary>

        /// <param name="fps"></param>

        public virtual void GameStart(int fps)

        {

            Fps = fps;

            GameMainStart();

        }

        /// <summary>

        /// ゲームループを中止します。

        /// </summary>

        public virtual void GameEnd()

        {

            IsRunning = false;

        }



        #endregion パブリックメソッド



        #region プライベートメソッド



        void GameMainStart()

        {

            IsRunning = true;

            try

            {

                timer.TimerInit(Fps);

                if (GameInit != null)

                    GameInit();

                if (GameLoad != null)

                    GameLoad();



                timer.TimerStart();



                GameLoop();

            }

            finally

            {

                if (GameExit != null)

                    GameExit();

            }

        }



        void GameLoop()

        {

            while (true)

            {

                Updating();

                Rendering();

                OtherProcessing();

                if (!IsRunning) break;

            }

        }

        int updateCounter;

        int measureTime;//計測するタイミングを保持する変数

        void Updating()

        {

            if (measureTime != Second)

            {

                measureTime = Second;

                UpdateFps = updateCounter;

                RenderFps = renderCounter;

                updateCounter = 0;

                renderCounter = 0;

            }

            updateCounter++;

            timer.BeforeUpdate();

            GameUpdate();

            timer.AfterUpdate();

        }

        int renderCounter;

        void Rendering()

        {

            if (timer.RenderOK)

            {

                renderCounter++;

                timer.BeforeRender();

                //GameRender();

                timer.AfterRender();

            }

        }



        void OtherProcessing()

        {

            timer.JudgeRenderOK();

            timer.Wait();

            timer.PauseCheck();

        }



        #endregion プライベートメソッド



    }

    /// <summary>

    /// FPSを安定させて動作させるためのタイマーを専用のスレッドで動作させるためのクラスです

    /// </summary>

    public class FPSTimer : FPSTimerNonThread

    {

        System.Threading.Thread thread;



        #region パブリックメソッド

        /// <summary>

        /// ゲームループを新しいスレッドで開始します

        /// </summary>

        public override void GameStart()

        {

            if (thread == null || !thread.IsAlive)

            {

                IsRunning = true;

                CreatNewThread();

                thread.Start();

            }

        }

        /// <summary>

        /// ゲームループを新しいスレッドで開始します。引数に指定した数がFPSに設定されます

        /// </summary>

        /// <param name="fps"></param>

        public override void GameStart(int fps)

        {

            Fps = fps;

            GameStart();

        }

        /// <summary>

        /// ゲームループを中止します

        /// </summary>

        public void Abort()

        {

            if (thread != null && thread.IsAlive)

            {

                IsRunning = false;

                thread.Abort();

            }

        }

        /// <summary>

        /// ゲームループを終了します。

        /// Abortと同じ処理をします

        /// </summary>

        public override void GameEnd()

        {

            Abort();

        }

        /// <summary>

        /// WindowsFormを利用する場合は必ず呼び出してください。

        /// 引数に指定したフォームのClosingイベントにスレッドを中止する処理を追加します

        /// </summary>

        /// <param name="form"></param>

        //public void UseWindowsForm(Form form)

        //{

        //    if (form != null)

        //        form.FormClosing += new FormClosingEventHandler(FormClosing);

        //}



        #endregion パブリックメソッド



        #region プライベートメソッド

        void CreatNewThread()

        {

            thread = new System.Threading.Thread(new System.Threading.ThreadStart(base.GameStart));

            thread.IsBackground = true;

        }



        //void FormClosing(object sender, FormClosingEventArgs e)

        //{

        //    Abort();

        //}

        #endregion プライベートメソッド

    }

}