using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nextension
{
    public class GuiCommandUIManager : MonoBehaviour
    {
        private enum TabType
        {
            Log,
            Saved
        }

        [SerializeField] private GameObject _mainView;
        [SerializeField] private InputField _cmdIF;
        [SerializeField] private NButton _runButton;
        [SerializeField] private NButton _clearLogButton;
        [SerializeField] private NButton _openCloseButton;
        [SerializeField] private InfiniteScrollRect _guiCmdUIView;

        [SerializeField] private EnumArrayValue<TabType, NButton> _tabButtons;

        private TabType _currentTab;
        private List<GuiCmdUIData> _logData = new();
        private List<GuiCmdUIData> _savedData = new();

        private void Start()
        {
            _guiCmdUIView.onCreateNewCell += (cell) =>
            {
                var guiCell = (GuiCmdUI)cell;
                guiCell.onRButtonClick += __onGuiCmdUI_R_ButtonClick;
                guiCell.onSButtonClick += __onGuiCmdUI_S_ButtonClick;
            };
            _openCloseButton.onButtonClickEvent.AddListener(() =>
            {
                __show(!_mainView.activeInHierarchy);
            });
            _runButton.onButtonClickEvent.AddListener(__runCmd);
            _clearLogButton.onButtonClickEvent.AddListener(() =>
            {
                _logData.Clear();
                if (_currentTab == TabType.Log)
                {
                    _guiCmdUIView.clear();
                }
            });

            foreach ((var tabType, var button) in _tabButtons)
            {
                button.onButtonClickEvent.AddListener(() => __selectTab(tabType));
            }
            __selectTab(TabType.Log);
            __show(false);
        }

        private void Update()
        {
            if (EventSystem.current && EventSystem.current.currentSelectedGameObject == _cmdIF.gameObject && Input.GetKeyUp(KeyCode.Return))
            {
                __runCmd(_cmdIF.text);
            }
        }

        private void OnDisable()
        {
            GuiCommand.storeSavedCommandToFile(__querySavedCmdCanDelete());
        }

        private void __onGuiCmdUI_R_ButtonClick(int index)
        {
            if (_currentTab == TabType.Log)
            {
                var cmdUIData = _logData[index];
                _cmdIF.SetTextWithoutNotify(cmdUIData.cmdInput);
            }
            else if (_currentTab == TabType.Saved)
            {
                var cmdUIData = _savedData[index];
                _cmdIF.SetTextWithoutNotify(cmdUIData.cmdInput);
            }
        }
        private void __onGuiCmdUI_S_ButtonClick(int index)
        {
            if (_currentTab == TabType.Log)
            {
                var cmdUIData = _logData[index];
                addSavedCmd(cmdUIData.cmdInput, true);
                _guiCmdUIView.getShowingCell(index).refreshCellData();
            }
            else if (_currentTab == TabType.Saved)
            {
                var cmdUIData = _savedData[index];
                __removeSaveCmd(cmdUIData.cmdInput);
            }
        }

        private void __selectTab(TabType selectedTab)
        {
            _currentTab = selectedTab;
            foreach ((var tabType, var button) in _tabButtons)
            {
                button.Interactable = selectedTab != tabType;
            }
            switch (selectedTab)
            {
                case TabType.Log:
                    {
                        __viewLogs();
                        break;
                    }
                case TabType.Saved:
                    {
                        __viewSaveds();
                        break;
                    }
            }
        }

        private void __show(bool isShow)
        {
            _mainView.setActive(isShow);
            if (isShow)
            {
                _openCloseButton.transform.GetChild(0).setScaleY(-1);
            }
            else
            {
                _openCloseButton.transform.GetChild(0).setScaleY(1);
                GuiCommand.storeSavedCommandToFile(__querySavedCmdCanDelete());
            }
        }

        private void __viewLogs()
        {
            _clearLogButton.setActive(true);
            _guiCmdUIView.clear();
            _guiCmdUIView.addRange(_logData);
        }
        private void __viewSaveds()
        {
            _clearLogButton.setActive(false);
            _guiCmdUIView.clear();
            _guiCmdUIView.addRange(_savedData);
        }

        private void __runCmd()
        {
            __runCmd(_cmdIF.text);
        }
        private void __runCmd(string cmdInput)
        {
            _cmdIF.SetTextWithoutNotify(string.Empty);
            if (cmdInput.isNullOrEmpty()) return;

            var errors = GuiCommand.runCmd(cmdInput);

            var cmdUIData = new GuiCmdUIData
            {
                checkTag = () => __getLogTag(cmdInput),
                cmdInput = cmdInput,
            };

            if (errors.Count > 0)
            {
                cmdUIData.descriptions = new string[errors.Count];
                for (int i = 0; i < errors.Count; i++)
                {
                    cmdUIData.descriptions[i] = errors[i].Message;
                }
            }

            _logData.Add(cmdUIData);

            if (_currentTab == TabType.Log)
            {
                _guiCmdUIView.add(cmdUIData);
            }
            else
            {
                __selectTab(TabType.Log);
            }
            try
            {
                if (EventSystem.current)
                {
                    _cmdIF.OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
            catch (Exception)
            {

            }
        }

        private GuiCmdUIData.Tag __getLogTag(string cmdInput)
        {
            return getSaveIndex(cmdInput) >= 0 ? GuiCmdUIData.Tag.Log_Saved : GuiCmdUIData.Tag.Log_NotSaved;
        }

        public int getSaveIndex(string cmdInput)
        {
            return _savedData.FindIndex(item => item.cmdInput == cmdInput);
        }

        public void addSavedCmd(string cmdInput, bool canDelete)
        {
            if (getSaveIndex(cmdInput) >= 0)
            {
                return;
            }

            GuiCmdUIData cmdUIData = new GuiCmdUIData
            {
                cmdInput = cmdInput,
            };
            if (canDelete)
            {
                cmdUIData.checkTag = GuiCmdUIData.Saved_CanDelete;
            }
            else
            {
                cmdUIData.checkTag = GuiCmdUIData.Saved_NotDelete;
            }
            _savedData.Add(cmdUIData);
            if (_currentTab == TabType.Saved)
            {
                _guiCmdUIView.add(cmdUIData);
            }
        }

        private void __removeSaveCmd(string cmdInput)
        {
            var index = getSaveIndex(cmdInput);
            if (index >= 0)
            {
                _savedData.RemoveAt(index);
                _guiCmdUIView.remove(index);
            }
        }

        private IEnumerable<string> __querySavedCmdCanDelete()
        {
            foreach (var saveData in _savedData)
            {
                if (saveData.checkTag() == GuiCmdUIData.Tag.Saved_CanDelete)
                {
                    yield return saveData.cmdInput;
                }
            }
        }
    }
}
