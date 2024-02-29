using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DamageTextBehavior : MonoBehaviour
{

    public GameObject weaknessTextPrefab; // Assign in the inspector
    public GameObject blockTextPrefab; // Assign in the inspector
    private GameObject weaknessTextObject;//these are instances
    private GameObject blockTextObject;
    public float riseSpeed = 1.0f;
    public float fadeSpeed = 1.0f;
    public float lifeTime = 1.5f;
    public float scaleTime = 0.5f;  // Time it takes for the entire scaling sequence
    public Vector2 randomOffsetRange = new Vector2(0.5f, 0.5f);  // Range for random offset

    [Header("Scale Curve")]
    public AnimationCurve scaleCurve;

    private TextMeshProUGUI textComponent;
    private Color originalColor;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        originalColor = textComponent.color;

        // Randomize initial position (except z-axis)
        transform.position += new Vector3(
            Random.Range(-randomOffsetRange.x, randomOffsetRange.x),
            Random.Range(-randomOffsetRange.y, randomOffsetRange.y),
            0
        );



    }
        public void InstantiateWeaknessText()
    {
        weaknessTextObject = Instantiate(weaknessTextPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform.parent);
    }

    public void InstantiateBlockText()
    {
        blockTextObject = Instantiate(blockTextPrefab, transform.position + Vector3.up * 1f, Quaternion.identity, transform.parent);
    }
    private GameObject InstantiateTextElement(GameObject prefab, Vector3 offset, bool movesRight)
    {
        GameObject textElement = Instantiate(prefab, transform.position + offset, Quaternion.identity, transform.parent);
        DamageTextBehavior textBehavior = textElement.GetComponent<DamageTextBehavior>();


        return textElement;
    }

    public IEnumerator ScaleSequence()
    {
        float elapsed = 0f;
        
        while (elapsed < scaleTime)
        {
            float t = elapsed / scaleTime;
            float currentScale = scaleCurve.Evaluate(t);

            transform.localScale = Vector3.one * currentScale;

            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        // After scaling, start the rise and fade behavior
        StartCoroutine(RiseAndFade());
    }

     private IEnumerator RiseAndFade()
    {
        float fadeStartTime = lifeTime - (1.0f / fadeSpeed);

        while (lifeTime > 0)
        {
            // Rising effect
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // Move weakness text to the right if it exists
            if ( weaknessTextObject)
            {
                weaknessTextObject.transform.position += Vector3.right * riseSpeed * Time.deltaTime;
            }

            // Fading effect...
            // Existing fading logic

            lifeTime -= Time.deltaTime;
            yield return null;
        }

        // Destroy all text objects
        if (weaknessTextObject) Destroy(weaknessTextObject);
        if (blockTextObject) Destroy(blockTextObject);
        Destroy(gameObject);
    }
}
