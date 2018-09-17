
namespace nangka {

    // Game ロジック用の定義
    public sealed class Define
    {
        //----------------------------------------------------------------
        // パラメータ
        //----------------------------------------------------------------
        public static int SHOWABLE_BLOCK = 2;


        //----------------------------------------------------------------
        // シーン名
        //----------------------------------------------------------------
        public static string SCENE_NAME_FADE = "ui_fade";


        //----------------------------------------------------------------
        // Entity クラス名
        //----------------------------------------------------------------
        public static string ENTITY_CNAME_FADE = "nangka.entity.EntityFade";
        public static string ENTITY_CNAME_DUNGEON = "nangka.entity.EntityDungeon";


        //----------------------------------------------------------------
        // Prefabパス名
        //----------------------------------------------------------------
        public static string RES_PATH_PREFAB_WALL = "Prefabs/Wall";


        //----------------------------------------------------------------
        // Textureパス名
        //----------------------------------------------------------------
        public static string RES_PATH_TEXTURE_WALL_BRICK_CEILING = "Textures/block02";
        public static string RES_PATH_TEXTURE_WALL_BRICK_SIDEWALK = "Textures/block02";
        public static string RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL = "Textures/block01";


        //----------------------------------------------------------------
        // Object名
        //----------------------------------------------------------------
        public static string OBJ_NAME_DUNGEON_ROOT = "Dungeon";
        public static string OBJ_NAME_DUNGEON_BLOCK_ROOT = "Block_";

    } //class Define

} //namespace nangka
