using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
public class camera : MonoBehaviour
{
    public Transform[] keypoints;
    public MeshFilter[] meshs;
    Camera cameraToCapture;  // キャプチャするカメラ
    int imageWidth = 640;   // 保存する画像の幅
    int imageHeight = 640;  // 保存する画像の高さ
    // public Vector3[] vertexPositions;

    void Start()
    {
        cameraToCapture = GetComponent<Camera>();
        StartCoroutine(CaptureAndSaveRoutine());
        colliderset();
    }


    void colliderset()
    {
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            // MeshRendererがアタッチされているGameObjectを取得
            GameObject obj = meshRenderer.gameObject;

            // すでにMeshColliderが存在しないか確認
            if (obj.GetComponent<MeshCollider>() == null)
            {
                // MeshColliderを追加
                MeshCollider meshCollider = obj.AddComponent<MeshCollider>();

                // 必要に応じてメッシュコライダーの設定を行う（例: 凸状設定）
                meshCollider.convex = true; // 凸状に設定
            }
        }
    }
    // string modelname = "fandataset";
     string datasetname = "loddataset";

    IEnumerator CaptureAndSaveRoutine()
    {
        string[] paths = {
    $"../{datasetname}/labels/train",
    $"../{datasetname}/labels/val",
    $"../{datasetname}/images/train",
    $"../{datasetname}/images/val"
};
        foreach (var path in paths)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


        int count = 0;
        int i = 0;
        while (true)
        {
            count += 1;
            yield return null;
            if (count % 100 == 0)
                this.transform.parent.position = new Vector3(Random.Range(-1.0f, 20.0f)*0.5f, 1.0f, Random.Range(-15.0f, 15.0f)*0.5f);

            transform.localPosition = GetRandomPosition();
            cameraToCapture.transform.LookAt(transform.parent.position);
            Vector3 rootpos = GetRandomPosition() * 2;
            rootpos.y = 1;
            // transform.root.localPosition = rootpos;


            string type = i % 10 != 0 ? "train" : "val";


            string name = $"{i}";//System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            (string text, int viewableCount) = keypoint(name);
            // if (viewableCount < 2) continue;
            i += 1;
            File.WriteAllText($"../{datasetname}/labels/{type}/{name}.txt", text);
            byte[] bytes = CaptureAndSave(name);
            File.WriteAllBytes($"../{datasetname}/images/{type}/{name}.png", bytes);
            print($"{type}/{name}");
            // break;


            yield return new WaitForSeconds(0.1f);  // 5秒待機
        }
    }
    (float center_x, float center_y, float width, float height) boundbox()
    {
        List<Vector3> vertexList = new List<Vector3>();
        foreach (var mesh in meshs)
        {


            // すべての頂点座標をグローバル座標に変換して列挙
            foreach (Vector3 vertex in mesh.mesh.vertices)
            {
                // ローカル座標をグローバル座標に変換
                Vector3 worldPosition = mesh.transform.TransformPoint(vertex);
                // Debug.Log("Vertex (World): " + worldPosition);
                vertexList.Add(cameraToCapture.WorldToViewportPoint(worldPosition));
                // Debug.DrawRay(worldPosition, new Vector3(0, 0.01f, 0), Color.white, 0.1f);

            }
        }
        float minX = vertexList.Min(v => v.x);
        float maxX = vertexList.Max(v => v.x);
        float minY = vertexList.Min(v => v.y);
        float maxY = vertexList.Max(v => v.y);

        float center_x = (minX + maxX) / 2;
        float center_y = 1f - (minY + maxY) / 2;
        float width = maxX - minX;
        float height = maxY - minY;

        // タプルを返す
        return (center_x, center_y, width, height);

    }


    (string text, int viewableCount) keypoint(string name)
    {
        int viewableCount = 0;

        List<float> pointList = new List<float>();
        pointList.Add(1);
        var (center_x, center_y, width, height) = boundbox();
        pointList.Add(center_x);
        pointList.Add(center_y);
        pointList.Add(width);
        pointList.Add(height);

        foreach (var keypoint in keypoints)
        {
            Vector3 keypointpos = cameraToCapture.WorldToViewportPoint(keypoint.position);
            print($"{keypoint.gameObject.name},{keypointpos}");
            if (isviewable(keypoint.position)||true)
            {

                pointList.Add(keypointpos.x);
                pointList.Add(1f - keypointpos.y);
                pointList.Add(2);
                viewableCount += 1;
            }
            else
            {
                pointList.Add(0);
                pointList.Add(0);
                pointList.Add(0);
            }
        }
        print(string.Join(" ", pointList));
        // File.WriteAllText(savePath, string.Join(" ", pointList));
        return (string.Join(" ", pointList) + "\n", viewableCount);
    }


    bool isviewable(Vector3 point)
    {
        Vector3 direction = point - cameraToCapture.transform.position;

        // レイキャストのためのRayを作成
        Ray ray = new Ray(cameraToCapture.transform.position, direction);

        // レイがヒットした情報を格納する変数
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 1f);
        // レイキャストを実行
        if (Physics.Raycast(ray, out hit))
        {
            // ヒットしたオブジェクトを取得
            GameObject hitObject = hit.collider.gameObject;

            // ヒットしたオブジェクトの情報をログに出力
            Debug.Log("Hit Object: " + hitObject.name);
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.blue, 1f); // ヒットした位置までのレイを描画
            print(hitObject.transform.root.name == "fan");
            if (hitObject.transform.root.name == "fan") return true;
        }
        return false;
    }



    public Vector3 GetRandomPosition()
    {
        var min = new Vector3(1, 1, 1) * -5;
        var max = new Vector3(1, 1, 1) * 5;
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);

        return new Vector3(x, y, z);
    }

    byte[] CaptureAndSave(string name)
    {



        // RenderTextureの作成
        RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        cameraToCapture.targetTexture = renderTexture;

        // カメラのビューをレンダリング
        cameraToCapture.Render();

        // RenderTextureをTexture2Dに転送
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        // RenderTextureの解除
        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Texture2DをPNG形式でファイルに保存
        byte[] bytes = texture.EncodeToJPG();
        return bytes;

    }
}
