using UnityEngine;

[CreateAssetMenu(fileName = "ItemTemplateDatabase", menuName = "Inventory/ItemTemplateDataBaseSO")]
public class ItemTemplateDataBaseSO : ScriptableObject
{
    public ItemTemplateSO[] ItemTemplates;

    public ItemTemplateSO GetItemTemplate(int templateID)
    {
        foreach (var template in ItemTemplates)
        {
            if (template.TemplateID == templateID)
            {
                return template;
            }
        }
        return null;
    }
}