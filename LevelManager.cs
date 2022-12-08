using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int amountOfPokemon;
    private int amountOfPokemonCaught;

    public float currentTime;
    public float endTime;

    private GUIManager GUI;
    
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        amountOfPokemon = GameObject.FindGameObjectsWithTag("Pokemon").Length;
        endTime = 15f;
        GUI = GameObject.Find("GUI").GetComponent<GUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= endTime)
        {
            endGame("before time ran out");
        }
    }

    public void removePokemon(GameObject pokemon, bool capture)
    {
        Destroy(pokemon);
        amountOfPokemon--;
        if (capture)
        {
            amountOfPokemonCaught++;
            Score.Instance.scoreNumber++;
            GUI.ReportToPlayer("Success!", amountOfPokemon + " remaining", 2.0f);
        }
        else
        {
            GUI.ReportToPlayer("A Pancham escaped!", amountOfPokemon + " remaining", 2.0f);
        }
        if (amountOfPokemon <= 0)
        {
            endGame("before they all escaped");
        }
    }

    private void endGame(string reason)
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        if (amountOfPokemonCaught == 1)
        {
            GUI.ReportToPlayer("Well done!", "You have captured " + amountOfPokemonCaught + " Pancham ", reason);
        }
        else
        {
            GUI.ReportToPlayer("Well done!", "You have captured " + amountOfPokemonCaught + " Panchams ", reason);
        }
    }
}
