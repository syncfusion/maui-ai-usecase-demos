package crc6481e5b28a6b4dd87a;


public class NativeSnapLayout_FlingGestureListener
	extends android.view.GestureDetector.SimpleOnGestureListener
	implements
		mono.android.IGCUserPeer
{

	public NativeSnapLayout_FlingGestureListener ()
	{
		super ();
		if (getClass () == NativeSnapLayout_FlingGestureListener.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.NativeSnapLayout+FlingGestureListener, Syncfusion.Maui.Core", "", this, new java.lang.Object[] {  });
		}
	}

	public boolean onFling (android.view.MotionEvent p0, android.view.MotionEvent p1, float p2, float p3)
	{
		return n_onFling (p0, p1, p2, p3);
	}

	private native boolean n_onFling (android.view.MotionEvent p0, android.view.MotionEvent p1, float p2, float p3);

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
