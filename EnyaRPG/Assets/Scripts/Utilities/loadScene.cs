using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class loadScene : MonoBehaviour
{
    //Following code is a "localized" version of code from a youtube tutorial
    //source: https://www.youtube.com/watch?v=H_0I9h4gHco
    [SerializeField] private Slider slide;

    private AsyncOperation operate;
    private Canvas canvas;
    public GameObject mainMenu;
    public AnimationClip outro;


    private void Awake()
    {
        // Get reference to canvas and store
        canvas = GetComponentInChildren<Canvas>(true);
        // Keep around if needed for other loads
        // DontDestroyOnLoad(gameObject);
    }
    public void load(string scene)
    {


        // start coroutine for loading screen func (scene name from button)
        StartCoroutine(startLoad(scene));
    }

    private IEnumerator startLoad(string scene)
    {
        //play animation transition
        mainMenu.GetComponent<Animator>().SetTrigger("exit");
        //wait for animation to finish
        yield return new WaitForSeconds(outro.length);

        // enable canvas (set to inactive normally)
        canvas.gameObject.SetActive(true);

        // restart load progress
        updateUI(0);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        
        yield return null;
        // store scene manager progress
        operate = SceneManager.LoadSceneAsync(scene);
        
        // while loading isnt finished
        while (!operate.isDone)
        {
            updateUI(operate.progress);
            yield return null;
        }
        // once done loading, set to null and disable load canvas
        operate = null;
        canvas.gameObject.SetActive(false);
    }
    private void updateUI(float progress)
    {
        slide.value = progress;
    }
}