package mono.android.view;


public class ViewTreeObserver_OnGlobalLayoutListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.ViewTreeObserver.OnGlobalLayoutListener
{

	public ViewTreeObserver_OnGlobalLayoutListenerImplementor ()
	{
		super ();
		if (getClass () == ViewTreeObserver_OnGlobalLayoutListenerImplementor.class) {
			mono.android.TypeManager.Activate ("Android.Views.ViewTreeObserver+IOnGlobalLayoutListenerImplementor, Mono.Android", "", this, new java.lang.Object[] {  });
		}
	}

	public void onGlobalLayout ()
	{
		n_onGlobalLayout ();
	}

	private native void n_onGlobalLayout ();

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
