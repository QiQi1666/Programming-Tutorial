using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GUIManager : MonoBehaviour
{
    private GroupBox timerGb;
    private Label time;

    private GroupBox reportGb;
    private Label titleLabel;
    private Label line1Label;
    private Label line2Label;
    private Label line3Label;
    private Button returnButton;

    private LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        timerGb = root.Q<GroupBox>("Timer");
        time = root.Q<Label>("Time");
        
        reportGb = root.Q<GroupBox>("Report");
        titleLabel = root.Q<Label>("Title");
        line1Label = root.Q<Label>("Line1");
        line2Label = root.Q<Label>("Line2");
        line3Label = root.Q<Label>("Line3");
        returnButton = root.Q<Button>("Return");

        returnButton.clicked += ReturnButtonPressed;
        
        setDisplay(reportGb, false);
        setDisplay(timerGb, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (getDisplay(timerGb))
        {
            time.text = (levelManager.endTime - levelManager.currentTime).ToString("0");
        }
    }

    public void ReportToPlayer(string title, string line1, float reportTime)
    {
        if (getDisplay(reportGb))
        {
            CancelInvoke("HideReport");
        }
        setDisplay(timerGb, false);
        setDisplay(reportGb, true);
        titleLabel.text = title;
        line1Label.text = line1;
        titleLabel.visible = true;
        line1Label.visible = true;
        line2Label.visible = false;
        line3Label.visible = false;
        returnButton.visible = false;
        Invoke("hideReport", reportTime);
    }
    
    public void ReportToPlayer(string title, string line1, string line2)
    {
        if (getDisplay(reportGb))
        {
            CancelInvoke("HideReport");
        }
        setDisplay(timerGb, false);
        setDisplay(reportGb, true);
        titleLabel.text = title;
        line1Label.text = line1;
        line2Label.text = line2;
        line3Label.text = "Score: " + Score.Instance.scoreNumber;
        titleLabel.visible = true;
        line1Label.visible = true;
        line2Label.visible = true;
        line3Label.visible = true;
        returnButton.visible = true;
        returnButton.text = "Return to Start";
    }

    private void HideReport()
    {
        setDisplay(timerGb, true);
        setDisplay(reportGb, false);
    }
    
    private void ReturnButtonPressed()
    {
        SceneManager.LoadScene("Scenes/Start");
    }

    private void setDisplay(GroupBox gb, bool display)
    {
        gb.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private bool getDisplay(GroupBox gb)
    {
        if (gb.style.display == DisplayStyle.Flex)
        {
            return true;
        }
        return false;
    }
}
