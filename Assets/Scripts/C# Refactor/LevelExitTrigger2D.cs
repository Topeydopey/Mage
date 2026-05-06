using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class LevelExitTrigger2D : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private bool loadNextScene = true;
    [SerializeField] private int sceneBuildIndex = -1;

    [Header("Trigger Rules")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableAfterUse = true;

    private bool used;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("No SceneTransitionManager found in the scene.");
            return;
        }

        used = true;

        if (disableAfterUse)
            GetComponent<Collider2D>().enabled = false;

        if (loadNextScene)
        {
            SceneTransitionManager.Instance.LoadNextScene();
        }
        else
        {
            SceneTransitionManager.Instance.LoadScene(sceneBuildIndex);
        }
    }
}