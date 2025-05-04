using UnityEngine;

public class FPD_SingleLineTwoPropsAttribute : PropertyAttribute
{
    public string PropName;
    public int LabelWidth;
    public int SecLabelWidth;
    public int MiddlePadding;
    public int UpPadding;
    public int AddSecondPropWidth;

    public FPD_SingleLineTwoPropsAttribute(string propName, int labelWidth = 0, int secondPropLabelWidth = 0, int middlePadding = 10, int addSecondPropWidth = 0, int upPadding = 0)
    {
        PropName = propName;
        LabelWidth = labelWidth;
        MiddlePadding = middlePadding;
        SecLabelWidth = secondPropLabelWidth;
        AddSecondPropWidth = addSecondPropWidth;
        UpPadding = upPadding;
    }

}
