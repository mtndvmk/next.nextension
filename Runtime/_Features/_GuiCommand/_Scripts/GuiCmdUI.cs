using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    public class GuiCmdUI : InfiniteCell<GuiCmdUIData>
    {
        [SerializeField] private Text _cmdText;
        [SerializeField] private NButton _r_button;
        [SerializeField] private NButton _s_button;

        public Action<int> onRButtonClick;
        public Action<int> onSButtonClick;

        protected void Awake()
        {
            _r_button.onButtonClickEvent.AddListener(() => { onRButtonClick?.Invoke(CellIndex); });
            _s_button.onButtonClickEvent.AddListener(() => { onSButtonClick?.Invoke(CellIndex); });
        }

        private void setData(GuiCmdUIData guiCmdUIData)
        {
            var text = $">>> <color=green>{guiCmdUIData.cmdInput}</color>";

            var tag = guiCmdUIData.checkTag();

            if (tag == GuiCmdUIData.Tag.Saved_CanDelete || tag == GuiCmdUIData.Tag.Saved_NotDelete)
            {
                var description = GuiCommand.getCmdInfo(guiCmdUIData.cmdInput);
                text += $"\n\t+> <color=grey><size=25>{description}</size></color>";
            }
            else
            {
                if (guiCmdUIData.isDone)
                {
                    text += $"\n\t+> <color=green><size=25>DONE</size></color>";
                }
                else
                {
                    foreach (string description in guiCmdUIData.descriptions)
                    {
                        text += $"\n\t+> <color=red><size=25>{description}</size></color>";
                    }
                }
            }
            _cmdText.text = text;

            switch (tag)
            {
                case GuiCmdUIData.Tag.Log_Saved:
                case GuiCmdUIData.Tag.Saved_NotDelete:
                    {
                        _s_button.setActive(false);
                        break;
                    }
                case GuiCmdUIData.Tag.Log_NotSaved:
                    {
                        _s_button.setActive(true);
                        _s_button.GetComponentInChildren<Text>().text = "+";
                        break;
                    }
                case GuiCmdUIData.Tag.Saved_CanDelete:
                    {
                        _s_button.setActive(true);
                        _s_button.GetComponentInChildren<Text>().text = "×";
                        break;
                    }
            }
        }
        protected override void onUpdateCellData(int index, GuiCmdUIData cellData)
        {
            setData(cellData);
            transform.rectTransform().markLayoutForRebuild(true);
            var size = CellData.CellSize;
            size.y = transform.rectTransform().rect.size.y;
            CellData.setSize(size);
        }
    }

    public class GuiCmdUIData : InfiniteCellData<GuiCmdUIData>
    {
        public enum Tag
        {
            Log_NotSaved,
            Log_Saved,
            Saved_NotDelete,
            Saved_CanDelete
        }

        public bool isDone => descriptions.Length == 0;

        public Func<Tag> checkTag;
        public string cmdInput;
        public string[] descriptions = Array.Empty<string>();

        public static Func<Tag> Saved_NotDelete = () => Tag.Saved_NotDelete;
        public static Func<Tag> Saved_CanDelete = () => Tag.Saved_CanDelete;
    }
}
