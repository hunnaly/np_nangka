using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class ui : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> objectTable;

    void Awake()
    {
        this.CheckAndAddEventSystem();
        this.ChangeWorldCameraForCanvas();
    }

	// Use this for initialization
	void Start () {
		
	}


    private void CheckAndAddEventSystem()
    {
        if (EventSystem.current == null)
        {
            var instance = new GameObject("EventSystem");
            EventSystem.current = instance.AddComponent<EventSystem>();
            instance.AddComponent<StandaloneInputModule>();
        }
    }
	
    private void ChangeWorldCameraForCanvas()
    {
        do
        {
            var parentScene = SceneManager.GetSceneByName("main");
            if (parentScene.IsValid() == false) break;

            var rootCanvasParentScene = parentScene.GetRootGameObjects().First(obj => obj.GetComponent<Canvas>() != null).GetComponent<Canvas>();
            if (rootCanvasParentScene == null) break;

            var rootCanvas = GetComponent<Canvas>();
            if (rootCanvas == null) break;

            if (rootCanvas.worldCamera != null)
            {
                Object.Destroy(rootCanvas.worldCamera.gameObject);
                rootCanvas.worldCamera = null;
            }

            rootCanvas.worldCamera = rootCanvasParentScene.worldCamera;
        }
        while (false);
    }
}
