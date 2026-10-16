package crc64f5844edce43316e7;


public class OnTouchListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnTouchListener
{

	public OnTouchListener ()
	{
		super ();
		if (getClass () == OnTouchListener.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Rotator.OnTouchListener, Syncfusion.Maui.Core", "", this, new java.lang.Object[] {  });
		}
	}

	public OnTouchListener (android.content.Context p0, crc64f5844edce43316e7.PlatformRotator p1)
	{
		super ();
		if (getClass () == OnTouchListener.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Rotator.OnTouchListener, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Syncfusion.Maui.Core.Rotator.PlatformRotator, Syncfusion.Maui.Core", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public boolean onTouch (android.view.View p0, android.view.MotionEvent p1)
	{
		return n_onTouch (p0, p1);
	}

	private native boolean n_onTouch (android.view.View p0, android.view.MotionEvent p1);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
