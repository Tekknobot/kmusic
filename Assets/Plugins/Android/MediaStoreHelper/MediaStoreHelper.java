package com.example.mediastorehelper;

import android.app.Activity;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.os.Environment;
import android.provider.MediaStore;
import java.io.File; // ✅ FIX: Import java.io.File
import java.io.OutputStream;
import java.io.IOException;

public class MediaStoreHelper {
    private Context context;

    // Constructor receives Unity's activity.
    public MediaStoreHelper(Activity activity) {
        this.context = activity.getApplicationContext();
    }

    /**
     * Writes binary data (e.g., WAV file) to the Music directory.
     * If the file already exists, it is overwritten.
     *
     * @param folderPath The absolute path of the target folder (e.g., /storage/emulated/0/Music/Chops).
     * @param fileName   The file name to be created (e.g., "MyChop.wav").
     * @param data       The binary data to write.
     */
    public void writeToMusicDirectory(String folderPath, String fileName, byte[] data) {
        try {
            // Convert the absolute folderPath to a relative path required by MediaStore.
            String relativeFolder = getRelativeFolderPath(folderPath);
            
            // Delete the file if it already exists (to prevent duplicates).
            deleteFile(folderPath, fileName);

            ContentValues values = new ContentValues();
            values.put(MediaStore.MediaColumns.DISPLAY_NAME, fileName);
            values.put(MediaStore.MediaColumns.MIME_TYPE, "audio/wav");
            values.put(MediaStore.MediaColumns.RELATIVE_PATH, relativeFolder);

            Uri externalUri = MediaStore.Audio.Media.EXTERNAL_CONTENT_URI;
            Uri fileUri = context.getContentResolver().insert(externalUri, values);

            if (fileUri != null) {
                try (OutputStream outStream = context.getContentResolver().openOutputStream(fileUri)) {
                    if (outStream != null) {
                        outStream.write(data);
                        outStream.flush();
                    }
                }
            } else {
                System.err.println("MediaStoreHelper: Failed to create URI for file: " + fileName);
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /**
     * Converts an absolute folder path to a MediaStore-relative path.
     *
     * @param folderPath The absolute path (e.g., "/storage/emulated/0/Music/Chops").
     * @return The MediaStore-relative path (e.g., "Music/Chops/").
     */
    private String getRelativeFolderPath(String folderPath) {
        String musicFolder = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_MUSIC).getAbsolutePath();
        String relativeFolder = Environment.DIRECTORY_MUSIC; 

        if (folderPath.startsWith(musicFolder)) {
            relativeFolder += folderPath.substring(musicFolder.length());
        }

        // Ensure the relative path ends with a slash.
        if (!relativeFolder.endsWith("/")) {
            relativeFolder += "/";
        }

        return relativeFolder;
    }

    /**
     * Checks if a folder exists in the Music directory.
     * MediaStore does not directly support folder existence checks,
     * so this method checks for any file in the folder.
     *
     * @param folderPath The absolute folder path.
     * @return true if any file exists in the folder, false otherwise.
     */
    public boolean folderExists(String folderPath) {
        String relativeFolder = getRelativeFolderPath(folderPath);

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
    }

    /**
     * Creates a folder in the Music directory.
     * MediaStore does not provide an explicit API for folder creation,
     * so this method writes a temporary file, then deletes it.
     *
     * @param folderPath The absolute folder path.
     */
    public void createFolder(String folderPath) {
        String dummyFileName = "temp.txt";
        byte[] dummyData = new byte[]{1};  // Smallest possible file.

        // Write a dummy file and delete it, forcing folder creation.
        writeToMusicDirectory(folderPath, dummyFileName, dummyData);
        deleteFile(folderPath, dummyFileName);
    }

    /**
     * Checks if a file exists in the specified Music directory folder.
     *
     * @param folderPath The folder path.
     * @param fileName   The file name.
     * @return true if the file exists, false otherwise.
     */
    public boolean fileExists(String folderPath, String fileName) {
        String relativeFolder = getRelativeFolderPath(folderPath);
        
        String[] projection = { MediaStore.MediaColumns._ID };
        String selection = MediaStore.MediaColumns.RELATIVE_PATH + "=? AND " + MediaStore.MediaColumns.DISPLAY_NAME + "=?";
        String[] selectionArgs = { relativeFolder, fileName };

        Uri queryUri = MediaStore.Files.getContentUri("external");
        Cursor cursor = context.getContentResolver().query(queryUri, projection, selection, selectionArgs, null);

        boolean exists = (cursor != null && cursor.getCount() > 0);
        if (cursor != null) {
            cursor.close();
        }
        return exists;
    }

    /**
     * Deletes a file from the Music directory.
     *
     * @param folderPath The folder path.
     * @param fileName   The file name.
     */
    public void deleteFile(String folderPath, String fileName) {
        try {
            String relativeFolder = getRelativeFolderPath(folderPath);

            String selection = MediaStore.MediaColumns.RELATIVE_PATH + "=? AND " + MediaStore.MediaColumns.DISPLAY_NAME + "=?";
            String[] selectionArgs = { relativeFolder, fileName };

            Uri queryUri = MediaStore.Files.getContentUri("external");
            int deleted = context.getContentResolver().delete(queryUri, selection, selectionArgs);

            if (deleted > 0) {
                System.out.println("MediaStoreHelper: Deleted file " + fileName);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
