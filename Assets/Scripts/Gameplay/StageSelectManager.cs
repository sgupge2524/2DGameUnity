using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    public void LoadStage(int stageNumber)
    {
        SceneManager.LoadScene("Stage" + stageNumber);
    }
}