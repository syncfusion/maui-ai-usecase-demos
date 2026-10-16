package crc6486af4e8104107bae;


public class AppCompatEditTextExt
	extends crc6452ffdc5b34af3a0f.MauiAppCompatEditText
	implements
		mono.android.IGCUserPeer
{

	public AppCompatEditTextExt (android.content.Context p0)
	{
		super (p0);
		if (getClass () == AppCompatEditTextExt.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Platform.AppCompatEditTextExt, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}

	public AppCompatEditTextExt (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == AppCompatEditTextExt.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Platform.AppCompatEditTextExt, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public AppCompatEditTextExt (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == AppCompatEditTextExt.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Platform.AppCompatEditTextExt, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}

	public android.view.inputmethod.InputConnection onCreateInputConnection (android.view.inputmethod.EditorInfo p0)
	{
		return n_onCreateInputConnection (p0);
	}

	private native android.view.inputmethod.InputConnection n_onCreateInputConnection (android.view.inputmethod.EditorInfo p0);

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
