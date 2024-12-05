using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class background : MonoBehaviour
{
    // Start is called before the first frame update
    public Texture[] textures;
    private Renderer objectRenderer;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        StartCoroutine(ChangeTexture());
    }

    // Update is called once per frame
   

    IEnumerator ChangeTexture()
    {
        while (true) // 無限ループでテクスチャを切り替え続ける
        {
            foreach (var texture in textures)
            {

                // 次のテクスチャを設定
                objectRenderer.material.mainTexture = texture;


                // 5秒待機
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
