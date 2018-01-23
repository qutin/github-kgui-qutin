using KGUI;

public class MyWindow : Window
{
    public MyWindow(AppContext context):base(context)
    {
        //contentPane = UIPackage.CreateObject("Basic", "ComboBox") as GComponent;
        //GComboBox cb = contentPane.asComboBox;
        //cb.items = new string[] { "aaa", "bbb", "ccc", "dddd" };

        contentPane = UIPackage.CreateObject("Common", "输入帐号下拉框") as GComponent;
    }
}
