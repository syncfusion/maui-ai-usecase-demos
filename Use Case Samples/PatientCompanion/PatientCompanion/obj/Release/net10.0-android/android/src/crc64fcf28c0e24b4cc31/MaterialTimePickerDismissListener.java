package crc64fcf28c0e24b4cc31;


public class MaterialTimePickerDismissListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.content.DialogInterface.OnDismissListener
{

	public MaterialTimePickerDismissListener ()
	{
		super ();
		if (getClass () == MaterialTimePickerDismissListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Handlers.MaterialTimePickerDismissListener, Microsoft.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public void onDismiss (android.content.DialogInterface p0)
	{
		n_onDismiss (p0);
	}

	private native void n_onDismiss (android.content.DialogInterface p0);

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
