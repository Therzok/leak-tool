using ObjCRuntime;

namespace XamMacLeaks;

public partial class ViewController : NSViewController
{
	readonly List<NSObject> _leaks = new();

	protected ViewController(NativeHandle handle) : base(handle)
	{
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();


		// Do any additional setup after loading the view.

		_ = JustKeepLeaking();
	}

	async Task JustKeepLeaking()
	{
		long iter = 0;
		while (true)
		{
			iter += 1;

			await Task.Delay(50);

			for (int i = 0; i < 10000; ++i)
			{
				_leaks.Add(new MyObject());
				_ = new CycleObject();
				_ = new CycleObjectNative();
			}
		}
	}

	class MyObject : NSView { }
	class CycleObjectNative : NSView
	{
		[Export("selfNative")]
		public CycleObjectNative SelfCycle { get; }

		public CycleObjectNative()
		{
			SelfCycle = this;
		}

	}
	class CycleObject : NSView
	{
		public CycleObject SelfCycle { get; }

		public CycleObject()
		{
			SelfCycle = this;
		}
	}

	public override NSObject RepresentedObject
	{
		get => base.RepresentedObject;
		set
		{
			base.RepresentedObject = value;

			// Update the view, if already loaded.
		}
	}
}