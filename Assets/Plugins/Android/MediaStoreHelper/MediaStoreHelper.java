package com.example.mediastorehelper;

import android.app.Activity;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.os.Environment;
import android.provider.MediaStore;
import java.io.OutputStream;

public class MediaStoreHelper {
    private Context context;

    // Constructor receives Unity's activity.
    public MediaStoreHelper(Activity activity) {
        this.context = activity.getApplicationContext();
    }

    /**
     * Writes binary data (for example, WAV data) to the Music directory in the specified folder.
     *
     * @param folderPath The absolute path of the target folder (e.g. /storage/emulated/0/Music/Chops).
     * @param fileName   The file name to be created (e.g. "MyChop.wav").
     * @param data       The binary data to write.
     */
    public void writeToMusicDirectory(String folderPath, String fileName, byte[] data) {
        try {
            // Convert the absolute folderPath to a relative path as required by MediaStore.
            String musicFolder = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MUSIC).getAbsolutePath();
            String relativeFolder;
            if (folderPath.startsWith(musicFolder)) {
                // Remove the Music folder absolute path from folderPath.
                relativeFolder = folderPath.substring(musicFolder.length());
                // Remove any leading slash.
                if (relativeFolder.startsWith("/")) {
                    relativeFolder = relativeFolder.substring(1);
                }
                // Prepend the standard Music directory.
                relativeFolder = Environment.DIRECTORY_MUSIC + "/" + relativeFolder;
            } else {
                relativeFolder = Environment.DIRECTORY_MUSIC;
            }
            // Ensure the relative path ends with a slash.
            if (!relativeFolder.endsWith("/")) {
                relativeFolder += "/";
            }

            ContentValues values = new ContentValues();
            values.put(MediaStore.MediaColumns.DISPLAY_NAME, fileName);
            // For WAV files, use the MIME type "audio/wav".
            values.put(MediaStore.MediaColumns.MIME_TYPE, "audio/wav");
            values.put(MediaStore.MediaColumns.RELATIVE_PATH, relativeFolder);

            // Use the Audio collection URI to insert audio files.
            Uri externalUri = MediaStore.Audio.Media.EXTERNAL_CONTENT_URI;
            Uri fileUri = context.getContentResolver().insert(externalUri, values);

            if (fileUri != null) {
                try (OutputStream outStream = context.getContentResolver().openOutputStream(fileUri)) {
                    if (outStream != null) {
                        outStream.write(data);
                        outStream.flush();
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * Checks if any file exists in the specified folder in the Music directory.
     * Note: There is no direct API to check for folder existence in MediaStore;
     * here we query for any file with the given relative path.
     *
     * @param folderPath The absolute folder path (e.g. /storage/emulated/0/Music/Chops).
     * @return true if files exist in that folder; false otherwise.
     */
    public boolean folderExists(String folderPath) {
        try {
            String musicFolder = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MUSIC).getAbsolutePath();
            String relativeFolder;
            if (folderPath.startsWith(musicFolder)) {
                relativeFolder = folderPath.substring(musicFolder.length());
                if (relativeFolder.startsWith("/")) {
                    relativeFolder = relativeFolder.substring(1);
                }
                relativeFolder = Environment.DIRECTORY_MUSIC + "/" + relativeFolder;
            } else {
                relativeFolder = Environment.DIRECTORY_MUSIC;
            }
            // Ensure relative path ends with a slash.
            if (!relativeFolder.endsWith("/")) {
                relativeFolder += "/";
            }

            String[] projection = { MediaStore.MediaColumns._ID };
            String selection = MediaStore.MediaColumns.RELATIVE_PATH + "=?";
            String[] selectionArgs = { relativeFolder };
            Uri queryUri = MediaStore.Files.getContentUri("external");
            Cursor cursor = context.getContentResolver().query(queryUri, projection, selection, selectionArgs, null);
            boolean exists = (cursor != null && cursor.getCount() > 0);
            if (cursor != null) {
                cursor.close();
            }
            return exists;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    /**
     * Creates a folder in the Music directory.
     * Note: MediaStore does not provide an API to explicitly create folders.
     * In most cases, inserting a file with a RELATIVE_PATH automatically creates the folder.
     * This method is provided as a stub if you wish to implement a workaround.
     *
     * @param folderPath The absolute folder path (e.g. /storage/emulated/0/Music/Chops).
     */
    public void createFolder(String folderPath) {
        // Workaround (optional): Insert a dummy file and immediately delete it,
        // which can force the creation of the folder.
        // For now, this method is a no-op.
    }
}
