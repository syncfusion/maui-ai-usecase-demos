package crc64fcf28c0e24b4cc31;


public class MaterialTimePickerPositiveButtonClickListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnClickListener
{

	public MaterialTimePickerPositiveButtonClickListener ()
	{
		super ();
		if (getClass () == MaterialTimePickerPositiveButtonClickListener.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.Handlers.MaterialTimePickerPositiveButtonClickListener, Microsoft.Maui", "", this, new java.lang.Object[] {  });
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
