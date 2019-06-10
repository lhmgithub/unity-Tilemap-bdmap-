using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace lhm
{
    /// <summary>
    /// 级别边缘索引
    /// </summary>
    public struct Level
    {
        public int indexR;
        public int indexL;
        public int indexT;
        public int indexB;
        public int widthR;
        public int widthL;
    }
    public enum MoveState_L
    {
        None,
        left,
        right,
    }
    public enum MoveState_T
    {
        None,
        buttom,
        top
    }
    public class RawImageV
    {
        public RawImage raw;
        public string url;

        public RawImageV(RawImage raw, string url)
        {
            this.raw = raw;
            this.url = url;
        }
    }
    public class Map : MonoBehaviour
    {

        public RectTransform container;
        public Text unit;
        public int size = 256;
        public Rect rect = new Rect(0, 0, 0, 0);
        public Vector2 lastVect_L = new Vector2(128, 0);
        public Vector2 lastVect_R = new Vector2(128, 0);
        public Vector2 lastVect_B = new Vector2(0, 128);
        public Vector2 lastVect_T = new Vector2(0, 128);
        public int row_L, row_R;
        public int col_T, col_B;
        RawImage image;
        int x = 0, y = 0;
        float lon = 0;
        float lat = 0;
        public int level = 0;
        float _scrollWheelValue;
        float during = 0.8f;
        float currentTime;
        bool scaleFlag;     
        Dictionary<int, string> zoomLevel = new Dictionary<int, string>() { { 3, "2000公里" }, { 4, "1000公里" }, { 5, "500公里" }, { 6, "200公里" }, { 7, "100公里" }, { 8, "50公里" }, { 9, "25公里" }, { 10, "20公里" }, { 11, "10公里" }, { 12, "5公里" }, { 13, "2公里" }, { 14, "1公里" }, { 15, "500米" }, { 16, "200米" }, { 17, "100米" }, { 18, "50米" } };
        Dictionary<int, Level> levelOffect = new Dictionary<int, Level>() {
        { 3,new Level (){ indexR =2,indexL=-3,indexT=2,indexB=-2,widthR=99,widthL=100} },{ 4,new Level (){ indexR =4,indexL=-5,indexT=4,indexB=-4,widthR=199,widthL=200} },
        { 5,new Level (){ indexR =9,indexL=-10,indexT=9,indexB=-8,widthR=142,widthL=143} },{ 6,new Level (){ indexR =19,indexL=-20,indexT=18,indexB=-16,widthR=28,widthL=29} },
        { 7,new Level (){ indexR =38,indexL=-39,indexT=37,indexB=-31,widthR=56,widthL=57} },{ 8,new Level (){ indexR =76,indexL=-77,indexT=74,indexB=-62,widthR=112,widthL=113} },
    };
        Dictionary<string, Texture2D> textureDic = new Dictionary<string, Texture2D>();
        Dictionary<string, RawImageV> rawDic = new Dictionary<string, RawImageV>();
        Dictionary<string, string> texture_rawDic = new Dictionary<string, string>();
        int r_L, r_R;
        int c_T, c_B;
        int centerX, centerY;
        int mul;
        MoveState_L mStateL;
        MoveState_T mStateT;
        BDMapTool BDMapTool = new BDMapTool();
        LatLngPoint latLngPoint;
        private void Start()
        {
            image = new GameObject().AddComponent<RawImage>();
            container.parent.GetComponent<DragImage>().RegisterDragEvent(UpdateTileOffectOfMove);
            latLngPoint = new LatLngPoint(0,0);
            r_L = row_L;
            r_R = row_R;
            c_T = col_T;
            c_B = col_B;
            LoadTileShell();
            InitMap();           
        }
        //重置瓦片网格
        void ResetTilePos()
        {
            int mm = 0;
         row_L  = r_L  ;
         row_R = r_R;
         col_T  = c_T  ;
         col_B  = c_B;
            container.anchoredPosition = Vector2.zero;
            for (int i = row_L; i <= row_R; i++)
            {
                for (int j = col_B; j <= col_T; j++)
                {
                    container.GetChild(mm).name = i + "_" + j;
                    RectTransform rt = container.GetChild(mm).GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(i * size, j * size);
                    rt.GetComponent<RawImage>().texture = null;
                    mm++;
                }
            }
           Tile tile= BDMapTool.GetTile(lon,lat,level);           
            centerX = tile.X;
            centerY = tile.Y;
        }
        void Update()
        {
            _scrollWheelValue = Input.GetAxis("Mouse ScrollWheel");
            currentTime += Time.deltaTime;
            if (_scrollWheelValue > 0 && currentTime > during && level < 8)
            {
                scaleFlag = true;
                level++;
            }
            else if (_scrollWheelValue < 0 && currentTime > during && level > 3)
            {
                scaleFlag = true;
                level--;
            }
            if (scaleFlag)
            {
                InitMap();
                currentTime = 0;
                scaleFlag = false;
            }
        }
        //首次加载网格
        void LoadTileShell()
        {
            for (int i = row_L; i <= row_R; i++)
            {
                for (int j = col_B; j <= col_T; j++)
                {
                    RawImage img = Instantiate<RawImage>(image);
                    img.rectTransform.SetParent(container);
                    img.rectTransform.sizeDelta = new Vector2(size, size);
                    img.rectTransform.anchoredPosition = new Vector2(i * size, j * size);
                    img.name = i + "_" + j;
                    GameObject go = new GameObject(i + "_" + j);
                    go.transform.SetParent(img.transform);
                    go.transform.localPosition = Vector3.zero;
                    img.raycastTarget = false;
                    Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGB24, false);
                    textureDic.Add(i + "_" + j, texture2D);
                }
            }
            container.GetComponent<RectTransform>().sizeDelta = new Vector2(row_R - row_L + 1, col_T - col_B + 1) * size;
            Vector2 vv = container.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(row_R - row_L + 1, col_T - col_B + 1) * size - new Vector2(size * 4, size * 4);
            rect = new Rect(-vv.x / 2, -vv.y / 2, vv.x / 2, vv.y / 2);
        }
        float pposX;
        float pposY;
        void InitMap()
        {
            int xV = 0;
            int yV = 0;
            int size = 0;
            mul = levelOffect[level].indexR - levelOffect[level].indexL + 1;
            int rem = 0;
            int div = 0;
            float xPos = 0;
            float yPos = 0;
            ResetTilePos();        
            Tile tile = BDMapTool.GetTile(latLngPoint.Lng, latLngPoint.Lat, level);
            x = tile.X;
            y = tile.Y;
            for (int i = row_L; i <= row_R; i++)
            {
                xV = x + i;
                rem = xV % mul;
                div = xV / mul;
                size = 256;
                if (xV < levelOffect[level].indexL)
                {
                    if (rem < levelOffect[level].indexL)
                    {
                        xV = rem + mul;
                        if (div == 0)
                            div = -1;
                        else if (div < 0)
                            div = div - 1;
                    }
                    else
                    {
                        xV = rem;
                    }
                    xPos = -((size / 2 - levelOffect[level].widthL) + (size / 2 - levelOffect[level].widthR) + size) * div;
                }
                else if (xV > levelOffect[level].indexR)
                {
                    if (rem > levelOffect[level].indexR)
                    {
                        xV = rem - mul;
                        if (div == 0)
                            div = 1;
                        else if (div > 0)
                            div = div + 1;
                    }
                    else
                    {
                        xV = rem;
                    }

                    xPos = -((size / 2 - levelOffect[level].widthL) + (size / 2 - levelOffect[level].widthR) + size) * div;
                }
                else
                {
                    xPos = 0;
                    yPos = 0;
                }
                for (int j = col_B; j <= col_T; j++)
                {
                    yV = y + j;
                    StartCoroutine(GetHttpTexture(container.Find(i + "_" + j).GetComponent<RawImage>(), "http://online1.map.bdimg.com/onlinelabel/?qt=tile" + $"&x={xV}&y={yV}&z={level}"));
                    container.Find(i + "_" + j).GetComponent<RectTransform>().anchoredPosition = container.Find(i + "_" + j).GetComponent<RectTransform>().anchoredPosition + new Vector2(xPos, yPos);
                }
            }
            lastVect_L = new Vector2(container.Find(row_L + "_" + col_T).GetComponent<RectTransform>().anchoredPosition.x, 0);
            lastVect_R = new Vector2(container.Find(row_R + "_" + col_T).GetComponent<RectTransform>().anchoredPosition.x, 0);
            lastVect_B = new Vector2(0, container.Find(row_L + "_" + col_B).GetComponent<RectTransform>().anchoredPosition.y);
            lastVect_T = new Vector2(0, container.Find(row_L + "_" + col_T).GetComponent<RectTransform>().anchoredPosition.y);
            unit.text = zoomLevel[level];
            UpdateTileOffectOfScale();
        }
        //缩放地图后更新瓦片
        void UpdateTileOffectOfScale()
        {
            int ll = 0;
            int tt = 0;
            float offect = 0;
            if (lastVect_L.x + container.anchoredPosition.x - size / 2 > rect.x)
            {
                mStateL = MoveState_L.left;
                offect = lastVect_L.x + container.anchoredPosition.x - size / 2 - rect.x;
            }
            else if (lastVect_R.x + container.anchoredPosition.x + size / 2 < rect.width)
            {
                mStateL = MoveState_L.right;
                offect = lastVect_L.x + container.anchoredPosition.x - size / 2 - rect.x;
            }
            else
                mStateL = MoveState_L.None;
            float reml = Mathf.Abs(offect) / size;
            int remlv = Mathf.FloorToInt(reml);
            if (reml == remlv)
                ll = remlv;
            else
                ll = remlv + 1;
            mStateT = MoveState_T.None;
            UpdateTile(mStateL, mStateT, ll, tt);
        }
        //移动地图后更新瓦片
        void UpdateTileOffectOfMove(Vector2 offect)
        {
            if (rawDic.Count > 0)
                rawDic.Clear();
            if (texture_rawDic.Count > 0)
                texture_rawDic.Clear();
            int tt = 0;
            float temp = 0;
            pposX = container.anchoredPosition.x;
            pposY = container.anchoredPosition.y;
            if (offect.x > 0)
            {
                temp = lastVect_L.x + pposX - size / 2 - rect.x;
                if (temp > 0)
                {
                    mStateL = MoveState_L.left;
                    SetTileXPos(x, y, row_L - 1, mStateL);
                }
                else
                    mStateL = MoveState_L.None;
            }
            else if (offect.x < 0)
            {
                if (lastVect_R.x + pposX + size / 2 < rect.width)
                {
                    mStateL = MoveState_L.right;
                    SetTileXPos(x, y, row_R + 1, mStateL);
                }
                else
                    mStateL = MoveState_L.None;
            }

            if (offect.y > 0)
            {
                if (lastVect_B.y + pposY - size / 2 > rect.y)
                {
                    mStateT = MoveState_T.buttom;
                    tt = SetOffectNumber(lastVect_B.y + pposY - size / 2 - rect.y);
                }
                else
                    mStateT = MoveState_T.None;
            }
            else if (offect.y < 0)
            {
                if (lastVect_T.y + pposY + size / 2 < rect.height)
                {
                    mStateT = MoveState_T.top;
                    tt = SetOffectNumber(lastVect_T.y + pposY + size / 2 - rect.height);
                }
                else
                    mStateT = MoveState_T.None;
            }
            UpdateTile(MoveState_L.None, mStateT, 0, tt);
            foreach (string ss in rawDic.Keys)
            {
                StartCoroutine(GetHttpTexture(rawDic[ss].raw, rawDic[ss].url));
            }
        }
        void UpdateTile(MoveState_L isLeft, MoveState_T isTop, int ll, int tt)
        {
            switch (isLeft)
            {
                case MoveState_L.left:
                    for (int i = 0; i < ll; i++)
                    {
                        SetTileXPos(x, y, row_L - 1, isLeft);
                    }
                    break;
                case MoveState_L.right:
                    for (int i = 0; i < ll; i++)
                    {
                        SetTileXPos(x, y, row_R + 1, isLeft);
                    }
                    break;
                case MoveState_L.None:
                    break;
            }
            switch (isTop)
            {
                case MoveState_T.buttom:
                    for (int i = 0; i < tt; i++)
                    {
                        SetTileYPos(x, y, col_B - 1, isTop);
                    }
                    break;
                case MoveState_T.top:
                    for (int i = 0; i < tt; i++)
                    {
                        SetTileYPos(x, y, col_T + 1, isTop);
                    }
                    break;
                case MoveState_T.None:
                    break;
            }
        }
        #region 设置单列或单行瓦片
        void SetTileXPos(int x, int y, int i, MoveState_L state)
        {
            int xV = 0;
            int yV = 0;
            int rem = 0;
            int div = 0;
            float xPos = 0;
            float yPos = 0;
            float hight = 0;
            xV = x + i;
            rem = xV % mul;
            div = xV / mul;
            float rectX = 0;
            if (state == MoveState_L.left)
            {
                if (xV == levelOffect[level].indexR + mul * div)
                {
                    xPos = (size / 2 - levelOffect[level].widthL) + (size / 2 - levelOffect[level].widthR) + size;
                }
                else if (xV == levelOffect[level].indexL + mul * div - 1)
                {
                    rectX = size - levelOffect[level].widthR;
                    xPos = (size / 2 - levelOffect[level].widthL) + (size / 2 - levelOffect[level].widthR) + size;
                }
                else if (xV == levelOffect[level].indexL + mul * div)
                {
                    rectX = size - levelOffect[level].widthL;
                }
                centerX--;
            }
            else if (state == MoveState_L.right)
            {
                if (xV == levelOffect[level].indexL + mul * div)
                {
                    xPos = -((size / 2 - levelOffect[level].widthL) + (size / 2 - levelOffect[level].widthR) + size);
                }
                else if (xV == levelOffect[level].indexR + mul * div + 1)
                {
                    xPos = -((size / 2 - levelOffect[level].widthL) + (size / 2 - levelOffect[level].widthR) + size);
                    rectX = size - levelOffect[level].widthL;
                }
                else if (xV == levelOffect[level].indexR + mul * div)
                {
                    rectX = size - levelOffect[level].widthR;
                }
                centerX++;
            }
            if (xV < levelOffect[level].indexL)
            {
                if (rem < levelOffect[level].indexL)
                {
                    xV = rem + mul;
                }
                else
                {
                    xV = rem;
                }
            }
            else if (xV > levelOffect[level].indexR)
            {
                if (rem > levelOffect[level].indexR)
                {
                    xV = rem - mul;
                }
                else
                {
                    xV = rem;
                }
            }
            RawImage img = null;
            string last = "";
            for (int j = col_B; j <= col_T; j++)
            {
                yV = y + j;
                hight = size * j;
                if (state == MoveState_L.left)
                {
                    last = row_R + "_" + j;
                    img = container.Find(row_R + "_" + j).GetComponent<RawImage>();
                    img.rectTransform.anchoredPosition = new Vector2(-size, hight) + new Vector2(xPos, yPos) + lastVect_L;
                }
                else if (state == MoveState_L.right)
                {
                    last = row_L + "_" + j;
                    img = container.Find(row_L + "_" + j).GetComponent<RawImage>();
                    img.rectTransform.anchoredPosition = new Vector2(size, hight) + new Vector2(xPos, yPos) + lastVect_R;

                }
                img.name = i + "_" + j;
                img.texture = null;
                if (!texture_rawDic.ContainsKey(img.transform.GetChild(0).name))
                {
                    texture_rawDic.Add(img.transform.GetChild(0).name, i + "_" + j);
                }
                else
                    rawDic.Remove(texture_rawDic[img.transform.GetChild(0).name]);
                rawDic.Add(i + "_" + j, new RawImageV(img, "http://online1.map.bdimg.com/onlinelabel/?qt=tile" + $"&x={xV}&y={yV}&z={level}"));

            }
            if (state == MoveState_L.left)
            {
                lastVect_L = new Vector2(img.rectTransform.anchoredPosition.x, 0);
                row_L--;
                row_R--;
                lastVect_R = new Vector2(container.Find(row_R + "_" + col_B).GetComponent<RectTransform>().anchoredPosition.x, 0);
                if (lastVect_L.x + pposX + rectX - size / 2 - rect.x > 0)
                    SetTileXPos(x, y, row_L - 1, state);
            }
            else if (state == MoveState_L.right)
            {
                lastVect_R = new Vector2(img.rectTransform.anchoredPosition.x, 0);
                row_R++;
                row_L++;
                lastVect_L = new Vector2(container.Find(row_L + "_" + col_B).GetComponent<RectTransform>().anchoredPosition.x, 0);
                if (lastVect_R.x + pposX - rectX + size / 2 < rect.width)
                    SetTileXPos(x, y, row_R + 1, state);
            }
            latLngPoint = BDMapTool.Tile2LngLat(centerX, centerY, level);
            lon = (float)latLngPoint.Lng;
            lat = (float)latLngPoint.Lat;
        }
        void SetTileYPos(int x, int y, int i, MoveState_T state)
        {
            int xV = 0;
            int yV = 0;
            float hight = 0;
            int rem = 0;
            int div = 0;
            float xPos = 0;
            float yPos = 0;
            yV = y + i;
            if (yV >= levelOffect[level].indexB && yV <= levelOffect[level].indexT)
            {
                RawImage img = null;
                for (int j = row_L; j <= row_R; j++)
                {
                    xV = x + j;
                    rem = xV % mul;
                    div = Mathf.Abs(xV / mul);
                    if (xV < levelOffect[level].indexL)
                    {
                        if (rem < levelOffect[level].indexL)
                        {
                            xPos = (2 * size - levelOffect[level].widthL - levelOffect[level].widthR) * (div + 1);
                            xV = rem + mul;
                        }
                        else
                        {
                            xPos = (2 * size - levelOffect[level].widthL - levelOffect[level].widthR) * div;
                            xV = rem;
                        }
                    }
                    else if (xV > levelOffect[level].indexR)
                    {
                        if (rem > levelOffect[level].indexR)
                        {
                            xPos = (levelOffect[level].widthL + levelOffect[level].widthR - 2 * size) * (div + 1);
                            xV = rem - mul;
                        }
                        else
                        {
                            xPos = (levelOffect[level].widthL + levelOffect[level].widthR - 2 * size) * div;
                            xV = rem;
                        }
                    }
                    else
                    {
                        xPos = 0;
                        yPos = 0;
                    }
                    hight = size * j;
                    if (state == MoveState_T.buttom)
                    {
                        img = container.Find(j + "_" + col_T).GetComponent<RawImage>();
                        img.rectTransform.anchoredPosition = new Vector2(hight, -size) + new Vector2(xPos, yPos) + lastVect_B;
                    }
                    else if (state == MoveState_T.top)
                    {
                        img = container.Find(j + "_" + col_B).GetComponent<RawImage>();
                        img.rectTransform.anchoredPosition = new Vector2(hight, size) + new Vector2(xPos, yPos) + lastVect_T;
                    }
                    img.name = j + "_" + i;
                    img.texture = null;
                    if (!texture_rawDic.ContainsKey(img.transform.GetChild(0).name))
                        texture_rawDic.Add(img.transform.GetChild(0).name, j + "_" + i);
                    else
                        rawDic.Remove(texture_rawDic[img.transform.GetChild(0).name]);
                    rawDic.Add(j + "_" + i, new RawImageV(img, "http://online1.map.bdimg.com/onlinelabel/?qt=tile" + $"&x={xV}&y={yV}&z={level}"));
                }
                if (state == MoveState_T.buttom)
                {
                    lastVect_B = new Vector2(0, img.rectTransform.anchoredPosition.y);
                    col_B--;
                    col_T--;
                    lastVect_T = new Vector2(0, container.Find(row_L + "_" + col_T).GetComponent<RectTransform>().anchoredPosition.y);
                    centerY--;
                }
                else if (state == MoveState_T.top)
                {
                    lastVect_T = new Vector2(0, img.rectTransform.anchoredPosition.y);
                    col_T++;
                    col_B++;
                    lastVect_B = new Vector2(0, container.Find(row_L + "_" + col_B).GetComponent<RectTransform>().anchoredPosition.y);
                    centerY++;
                }
            }
            else Debug.Log("超出矩形限制范围");
            latLngPoint = BDMapTool.Tile2LngLat(centerX, centerY, level);
            lon = (float)latLngPoint.Lng;
            lat = (float)latLngPoint.Lat;
        }
        int SetOffectNumber(float offect)
        {
            float remt = Mathf.Abs(offect) / size;
            int remtv = Mathf.FloorToInt(remt);
            if (remt == remtv)
                return remtv;
            else
                return remtv + 1;
        }
        #endregion
        IEnumerator GetHttpTexture(RawImage img, string url)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            float tt = Time.realtimeSinceStartup;
            yield return uwr.SendWebRequest();
            DownloadHandler downloadHandler = uwr.downloadHandler;
            if (uwr.isHttpError || uwr.isNetworkError) { }
            else
            {
                byte[] bts = downloadHandler.data;
                textureDic[img.transform.GetChild(0).name].LoadImage(bts);
                img.texture = textureDic[img.transform.GetChild(0).name];
                img.SetNativeSize();
            }
        }
    }
}

