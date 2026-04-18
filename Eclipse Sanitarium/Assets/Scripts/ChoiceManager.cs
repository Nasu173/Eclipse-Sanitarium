using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    [Header("按钮引用")]
    public Button acceptButton;
    public Button refuseButton;
    public GameObject choicePanel;

    [Header("任务配置")]
    public TaskData targetTask;      // 任务19

    
    private void Awake()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        BindAllButton();
    }

    private void BindAllButton()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAccept);

        if (refuseButton != null)
            refuseButton.onClick.AddListener(OnRefuse);
    }

    private void OnAccept()
    {
        Debug.Log("玩家选择了：接受转化");

        // 完成任务19，走分支0（第一个分支）
        if (TaskManager.Instance != null && targetTask != null)
        {
            TaskManager.Instance.CompleteTask(targetTask, 0);
        }

        Hide();
    }

    private void OnRefuse()
    {
        Debug.Log("玩家选择了：拒绝转化");

        // 完成任务19，走分支1（第二个分支）
        if (TaskManager.Instance != null && targetTask != null)
        {
            TaskManager.Instance.CompleteTask(targetTask, 1);
        }

        
        Hide();
    }

    public void Show()
    {
        if (choicePanel != null)
            choicePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
}