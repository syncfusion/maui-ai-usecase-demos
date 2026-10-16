package crc6479d6f9e48582dc67;


public class BackPressedHandler
	extends androidx.activity.OnBackPressedCallback
	implements
		mono.android.IGCUserPeer
{

	public BackPressedHandler (boolean p0)
	{
		super (p0);
		if (getClass () == BackPressedHandler.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Internals.BackPressedHandler, Syncfusion.Maui.Core", "System.Boolean, System.Private.CoreLib", this, new java.lang.Object[] { p0 });
		}
	}

	public void handleOnBackPressed ()
	{
		n_handleOnBackPressed ();
	}

	private native void n_handleOnBackPressed ();

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
