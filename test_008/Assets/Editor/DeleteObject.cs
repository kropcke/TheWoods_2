
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class DeleteObject : Editor
{

	static DeleteObject()
	{

		List<GameObject> rootObjects = new List<GameObject>();

		// get root objects in scene

		Scene scene = SceneManager.GetActiveScene();
		scene.GetRootGameObjects(rootObjects);

		// iterate root objects and do something

		for (int i = 0; i < rootObjects.Count; ++i)
		{

			GameObject gameObject = rootObjects[i];

			if (gameObject != null)
			{

				Debug.Log("Found the hidden passage");
                gameObject.hideFlags = HideFlags.None;

                if (gameObject.name == "PlayerRenderer1")
				{

					gameObject.hideFlags = HideFlags.None;
				}
                if (gameObject.name == "PlayerRenderer2")
                {

                    gameObject.hideFlags = HideFlags.None;
                }
            }
		}
	}
}