package crc6452ffdc5b34af3a0f;


public class AppbarLayoutExtensions_ScrollChangedListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.ViewTreeObserver.OnScrollChangedListener
{

	public AppbarLayoutExtensions_ScrollChangedListener ()
	{
		super ();
		if (getClass () == AppbarLayoutExtensions_ScrollChangedListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Platform.AppbarLayoutExtensions+ScrollChangedListener, Microsoft.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public void onScrollChanged ()
	{
		n_onScrollChanged ();
	}

	private native void n_onScrollChanged ();

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
