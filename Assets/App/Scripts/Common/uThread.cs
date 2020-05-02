using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class uThread : MonoBehaviour {
	private static bool _initialized = false;
	private static uThread _instance = null;
	private static object _lock = new object();

	protected static uThread Internal {
		get {
			lock(uThread._lock) {
				if(uThread._instance == null) {
					uThread._instance = Object.FindObjectOfType<uThread>();

					if(uThread._instance == null) {
						var go = new GameObject(typeof(uThread).Name);

						uThread._instance = go.AddComponent<uThread>();
					}

					uThread._instance.gameObject.name = typeof(uThread).Name;
					uThread._instance.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

					DontDestroyOnLoad(uThread._instance.gameObject);

					uThread._initialized = true;
				}
			}

			return uThread._instance;
		}
	}

	protected static Queue<System.Action> PendingTasks {
		get {
			return uThread.Internal.pendingTasks;
		}
		set {
			uThread.Internal.pendingTasks = value;
		}
	}

	protected static Queue<System.Action> ExecuteTasks {
		get {
			return uThread.Internal.executeTasks;
		}
		set {
			uThread.Internal.executeTasks = value;
		}
	}

	protected static List<uThread.DelayedTask> PendingDelayedTasks {
		get {
			return uThread.Internal.pendingDelayedTasks;
		}
		set {
			uThread.Internal.pendingDelayedTasks = value;
		}
	}

	protected static List<uThread.DelayedTask> ExecuteDelayedTasks {
		get {
			return uThread.Internal.executeDelayedTasks;
		}
		set {
			uThread.Internal.executeDelayedTasks = value;
		}
	}

	/// <summary>
	/// 1 per logical core
	/// </summary>
	/// <value>The optimimal max threads.</value>
	public static int OptimimalThreads {
		get {
			return uThread.Internal.optimalThreads;
		}
		protected set {
			uThread.Internal.optimalThreads = value;
		}
	}

	/// <summary>
	/// Gets or sets the max threads.
	/// </summary>
	/// <value>The max threads.</value>
	public static int MaxThreads {
		get {
			return uThread.Internal.maxThreads;
		}
		set {
			if(value < uThread.OptimimalThreads) value = uThread.OptimimalThreads;

			uThread.Internal.maxThreads = value;
		}
	}

	/// <summary>
	/// Gets the amount of threads in use.
	/// </summary>
	/// <value>The threads.</value>
	public static int Threads {
		get {
			return uThread.Internal.currentThreads;
		}
	}

	/// <summary>
	/// Gets the processor by name as reported by the system
	/// </summary>
	/// <value>The processor.</value>
	public static string Processor {
		get {
			return uThread.Internal.processor;
		}
		protected set {
			uThread.Internal.processor = value;
		}
	}

	/// <summary>
	/// Gets the frequency of the processor (i.e. 2.80 GHz)
	/// </summary>
	/// <value>The frequency.</value>
	public static string Frequency {
		get {
			return uThread.Internal.frequency;
		}
		protected set {
			uThread.Internal.frequency = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:uThread"/> executes exceptions on main thread.
	/// </summary>
	/// <value><c>true</c> if execute exceptions on main thread; otherwise, <c>false</c>.</value>
	public static bool RunExceptionsOnMainThread {
		get {
			return uThread.Internal.executeExceptionsOnMainThread;
		}
		set {
			uThread.Internal.executeExceptionsOnMainThread = value;
		}
	}

	protected Queue<System.Action> pendingTasks;
	protected Queue<System.Action> executeTasks;

	protected List<DelayedTask> pendingDelayedTasks;
	protected List<DelayedTask> executeDelayedTasks;

	private int optimalThreads;
	private int maxThreads;
	private int currentThreads;
	private string processor;
	private string frequency;
	private bool executeExceptionsOnMainThread = true;

	// Initialize once.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init() {
		if(!uThread._initialized) {
			uThread.OptimimalThreads = SystemInfo.processorCount;
			uThread.MaxThreads = uThread.OptimimalThreads;
			uThread.Processor = SystemInfo.processorType;
			uThread.Frequency = uThread.MhzToGhz(SystemInfo.processorFrequency);

			uThread.PendingTasks = new Queue<System.Action>();
			uThread.ExecuteTasks = new Queue<System.Action>();
			uThread.PendingDelayedTasks = new List<uThread.DelayedTask>();
			uThread.ExecuteDelayedTasks = new List<uThread.DelayedTask>();
			uThread.RunExceptionsOnMainThread = true;
		}
	}

	public static void RunCoroutine(IEnumerator routine, System.Action onComplete = null) {
		if(Internal == null) return;

		Internal.StartCoroutine(Internal.DoRunCoroutine(routine, onComplete));
	}

	IEnumerator DoRunCoroutine(IEnumerator routine, System.Action onComplete) {
		yield return Internal.StartCoroutine(routine);

		onComplete?.Invoke();
	}

	/// <summary>
	/// Executes on unity thread.
	/// </summary>
	/// <param name="task">Task.</param>
	public static void RunOnUnityThread(System.Action task) {
		uThread.RunOnUnityThread(task, 0.0F);
	}

	/// <summary>
	/// Executes on unity thread.
	/// </summary>
	/// <param name="task">Task.</param>
	/// <param name="delay">Delay.</param>
	public static void RunOnUnityThread(System.Action task, float delay) {
		uThread.RunOnUnityThread(task, delay, true);
	}

	/// <summary>
	/// Executes on unity thread.
	/// </summary>
	/// <param name="task">Task.</param>
	/// <param name="delay">Delay.</param>
	/// <param name="isRealTime">If set to <c>true</c> is real time.</param>
	public static void RunOnUnityThread(System.Action task, float delay, bool isRealTime) {
		if(delay <= 0.0F) {
			lock(uThread._lock) {
				uThread.PendingTasks.Enqueue(task);
			}
		} else {
			lock(uThread.PendingDelayedTasks) {
				uThread.PendingDelayedTasks.Add(
					new uThread.DelayedTask() {
						Task = task,
						RealExecutionTime = Time.realtimeSinceStartup + delay,
						IsRealTime = isRealTime,
						Delay = delay,
						ScaledTime = 0.0F
					}
				);
			}
		}
	}

	/// <summary>
	/// Runs the task async.
	/// </summary>
	/// <param name="task">Task.</param>
	public static void RunTaskAsync(System.Action task) {
		while(uThread.Threads >= uThread.MaxThreads) Thread.Sleep(1);
		Interlocked.Increment(ref uThread.Internal.currentThreads);
		ThreadPool.QueueUserWorkItem(uThread.RunTaskAsync_Internal, task);
	}

	/// <summary>
	/// Runs the task async internal.
	/// </summary>
	/// <param name="task">Task.</param>
	protected static void RunTaskAsync_Internal(object task) {
		try {
			((System.Action)task).Invoke();
		} catch(System.Exception e) {
			if(uThread.RunExceptionsOnMainThread) {
				uThread.RunOnUnityThread(
					() => {
						throw new System.Exception(e.Message);
					}
				);
			}
		} finally {
			Interlocked.Decrement(ref uThread.Internal.currentThreads);
		}
	}

	protected virtual void Awake() {
		if(uThread._instance != null) {
			if(uThread._instance != this)
				Destroy(this);
		}
	}

	protected virtual void Update() {
		this.HandleTasks();
		this.HandleDelayedTasks();
	}

	protected virtual void HandleTasks() {
		lock(this.pendingTasks) {
			this.executeTasks = new Queue<System.Action>(this.pendingTasks.ToArray());
			this.pendingTasks.Clear();
		}

		while(this.executeTasks.Count > 0) this.executeTasks.Dequeue().Invoke();
	}

	protected virtual void HandleDelayedTasks() {
		int executeSize = -1;

		lock(this.pendingDelayedTasks) {
			for(int i = 0, size = this.pendingDelayedTasks.Count; i < size; ++i) {
				var task = this.pendingDelayedTasks[i];

				if(task.IsRealTime) {
					if(Time.realtimeSinceStartup >= task.RealExecutionTime) {
						this.executeDelayedTasks.Add(task);
					}
				} else {
					task.ScaledTime += Time.deltaTime;

					if(task.ScaledTime >= task.Delay) {
						this.executeDelayedTasks.Add(task);
					}
				}
			}

			executeSize = this.executeDelayedTasks.Count;

			for(int i = 0; i < executeSize; ++i) {
				this.pendingDelayedTasks.Remove(this.executeDelayedTasks[i]);
			}
		}

		for(int i = 0; i < executeSize; ++i) {
			this.executeDelayedTasks[i].Task.Invoke();
		}

		this.executeDelayedTasks.Clear();
	}

	protected virtual void OnDestroy() {
		this.pendingTasks.Clear();
		this.pendingTasks = null;

		this.executeTasks.Clear();
		this.executeTasks = null;

		this.pendingDelayedTasks.Clear();
		this.pendingDelayedTasks = null;

		this.executeDelayedTasks.Clear();
		this.executeDelayedTasks = null;

		uThread._lock = null;
		uThread._instance = null;
		uThread._initialized = false;
	}

	public static string MhzToGhz(int frequency) {
		var ghz = frequency / 1000;
		var mhz = frequency % 1000;

		if(ghz > 0) {
			var value = (float)ghz + (mhz / 1000.0F);
			return string.Format("{0:0.00} GHz", value);
		}

		return frequency.ToString() + " MHz";
	}

	protected struct DelayedTask {
		public System.Action Task;
		public float RealExecutionTime;
		public bool IsRealTime;
		public float Delay;
		public float ScaledTime;
	}
}