
namespace nangka
{

    // Game ロジック用の定義
    public sealed class Define
    {
        //----------------------------------------------------------------
        // パラメータ
        //----------------------------------------------------------------
        public static int SHOWABLE_BLOCK = 3;


        //----------------------------------------------------------------
        // シーン名
        //----------------------------------------------------------------
        public static string SCENE_NAME_FADE = "ui_fade";
        public static string SCENE_NAME_FRAME = "ui_frame";
        public static string SCENE_NAME_DEV_ENTRANCE = "ui_dev_entrance";
        public static string SCENE_NAME_MAPEDITOR_CONSOLE = "ui_mapeditor_console";


        //----------------------------------------------------------------
        // Entity クラス名
        //----------------------------------------------------------------
        private static string ENTITY_PATH = "nangka.entity.";
        public static string ENTITY_CNAME_FADE = ENTITY_PATH + "EntityFade";
        public static string ENTITY_CNAME_DUNGEON = ENTITY_PATH + "EntityDungeon";
        public static string ENTITY_CNAME_TEXTURE_RESOURCES = ENTITY_PATH + "EntityTextureResources";
        public static string ENTITY_CNAME_FRAME = ENTITY_PATH + "EntityFrame";
        public static string ENTITY_CNAME_PLAYER = ENTITY_PATH + "EntityPlayer";
        public static string ENTITY_CNAME_PLAYER_DATA = ENTITY_PATH + "EntityPlayerData";
        public static string ENTITY_CNAME_MAP_DATA = ENTITY_PATH + "EntityMapData";
        public static string ENTITY_CNAME_STRUCTURE = ENTITY_PATH + "EntityStructure";
        public static string ENTITY_CNAME_MINIMAP = ENTITY_PATH + "EntityMiniMap";
        public static string ENTITY_CNAME_RECREATOR = ENTITY_PATH + "EntityRecreator";

        public static string ENTITY_CNAME_DEV_ENTRANCE = ENTITY_PATH + "EntityDevEntrance";
        public static string ENTITY_CNAME_MAPEDITOR_CONSOLE = ENTITY_PATH + "EntityMapEditorConsole";
        public static string ENTITY_CNAME_COMMON_DIALOG = ENTITY_PATH + "EntityCommonDialog";
        public static string ENTITY_CNAME_NEW_MAP = ENTITY_PATH + "EntityNewMap";


        //----------------------------------------------------------------
        // Prefabパス名
        //----------------------------------------------------------------
        public static string RES_PATH_PREFAB_WALL = "Prefabs/Wall";
        public static string RES_PATH_PREFAB_ARROW = "Prefabs/Arrow";
        public static string RES_PATH_PREFAB_COMMON_DIALOG = "Prefabs/CommonDialog";


        //----------------------------------------------------------------
        // Object名
        //----------------------------------------------------------------
        public static string OBJ_NAME_DUNGEON_ROOT = "Dungeon";
        public static string OBJ_NAME_DUNGEON_BLOCK_ROOT = "Block_";


        //----------------------------------------------------------------
        // Textureパス名 (ダミーデータ用)
        //----------------------------------------------------------------
        public static string RES_PATH_TEXTURE_WALL_BRICK_CEILING = "Textures/block02";
        public static string RES_PATH_TEXTURE_WALL_BRICK_SIDEWALK = "Textures/block02";
        public static string RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL = "Textures/block01";

    } //class Define

} //namespace nangka
