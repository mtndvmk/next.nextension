using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nextension
{
    public class GuiCmdUI : InfiniteCell<GuiCmdUIData>
    {
        [SerializeField] private Text _cmdText;
        [SerializeField] private NButton _self_button;
        [SerializeField] private NButton _r_button;
        [SerializeField] private NButton _s_button;

        public Action<int> onRButtonClick;
        public Action<int> onSButtonClick;

        protected void Awake()
        {
            _self_button.onButtonClickEvent.AddListener(() =>
            {
                var data = Data;
                data.isCollapsed = !data.isCollapsed;
                onUpdateCellData(CellIndex, data);
            });
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
                if (!guiCmdUIData.isCollapsed && guiCmdUIData.results != null)
                {
                    foreach (var result in guiCmdUIData.results)
                    {
                        if (result is Exception exception)
                        {
                            text += $"\n\t+> <color=red><size=25>{exception.Message}</size></color>";
                        }
                        else if (result is string log)
                        {
                            text += $"\n\t+> <color=grey><size=25>{log}</size></color>";
                        }
                        else
                        {
                            text += $"\n\t+> <color=grey><size=25>{result}</size></color>";
                        }
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
                        _s_button.GetComponentInChildren<Text>().text = "-";
                        break;
                    }
            }
        }
        protected override void onUpdateCellData(int index, GuiCmdUIData cellData)
        {
            setData(cellData);
            transform.rectTransform().markLayoutForRebuild(true);
            var size = CellData.cellSize;
            size.y = transform.rectTransform().rect.size.y;
            setSize(size);
        }
    }

    public class GuiCmdUIData
    {
        public enum Tag
        {
            Log_NotSaved,
            Log_Saved,
            Saved_NotDelete,
            Saved_CanDelete
        }

        public Func<Tag> checkTag;
        public string cmdInput;
        public bool isCollapsed;
        public List<object> results;

        public readonly static Func<Tag> Saved_NotDelete = () => Tag.Saved_NotDelete;
        public readonly static Func<Tag> Saved_CanDelete = () => Tag.Saved_CanDelete;
    }
}
