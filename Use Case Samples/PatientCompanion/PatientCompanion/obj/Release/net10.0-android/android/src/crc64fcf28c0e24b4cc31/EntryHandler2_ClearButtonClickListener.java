package crc64fcf28c0e24b4cc31;


public class EntryHandler2_ClearButtonClickListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnClickListener
{

	public EntryHandler2_ClearButtonClickListener ()
	{
		super ();
		if (getClass () == EntryHandler2_ClearButtonClickListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Handlers.EntryHandler2+ClearButtonClickListener, Microsoft.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public void onClick (android.view.View p0)
	{
		n_onClick (p0);
	}

	private native void n_onClick (android.view.View p0);

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
