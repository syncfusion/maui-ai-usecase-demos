package crc64fcf28c0e24b4cc31;


public class SearchBarHandler_QueryEditorTouchListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnTouchListener
{

	public SearchBarHandler_QueryEditorTouchListener ()
	{
		super ();
		if (getClass () == SearchBarHandler_QueryEditorTouchListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Handlers.SearchBarHandler+QueryEditorTouchListener, Microsoft.Maui", "", this, new java.lang.Object[] {  });
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
