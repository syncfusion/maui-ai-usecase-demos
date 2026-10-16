package crc6479d6f9e48582dc67;


public class PlatformScrollViewer
	extends android.widget.ScrollView
	implements
		mono.android.IGCUserPeer
{

	public PlatformScrollViewer (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == PlatformScrollViewer.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Internals.PlatformScrollViewer, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}

	public PlatformScrollViewer (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == PlatformScrollViewer.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Internals.PlatformScrollViewer, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public PlatformScrollViewer (android.content.Context p0)
	{
		super (p0);
		if (getClass () == PlatformScrollViewer.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Internals.PlatformScrollViewer, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}

	public boolean onInterceptTouchEvent (android.view.MotionEvent p0)
	{
		return n_onInterceptTouchEvent (p0);
	}

	private native boolean n_onInterceptTouchEvent (android.view.MotionEvent p0);

	public boolean onTouchEvent (android.view.MotionEvent p0)
	{
		return n_onTouchEvent (p0);
	}

	private native boolean n_onTouchEvent (android.view.MotionEvent p0);

	public void onOverScrolled (int p0, int p1, boolean p2, boolean p3)
	{
		n_onOverScrolled (p0, p1, p2, p3);
	}

	private native void n_onOverScrolled (int p0, int p1, boolean p2, boolean p3);

	public void scrollTo (int p0, int p1)
	{
		n_scrollTo (p0, p1);
	}

	private native void n_scrollTo (int p0, int p1);

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
