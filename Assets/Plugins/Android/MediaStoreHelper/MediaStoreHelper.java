package com.example.mediastorehelper;

import android.app.Activity;
import android.content.ContentValues;
import android.content.Context;
import android.net.Uri;
import android.os.Environment;
import android.provider.MediaStore;
import java.io.OutputStream;

public class MediaStoreHelper {
    private Context context;

    // Constructor receives Unity's activity
    public MediaStoreHelper(Activity activity) {
        this.context = activity.getApplicationContext();
    }

    // Method to write text to the Music directory
    public void writeToMusicDirectory(String fileName, String content) {
        ContentValues values = new ContentValues();
        values.put(MediaStore.MediaColumns.DISPLAY_NAME, fileName);
        values.put(MediaStore.MediaColumns.MIME_TYPE, "text/plain");
        values.put(MediaStore.MediaColumns.RELATIVE_PATH, Environment.DIRECTORY_MUSIC);

        Uri externalUri = MediaStore.Files.getContentUri("external");
        Uri fileUri = context.getContentResolver().insert(externalUri, values);

        try (OutputStream outStream = context.getContentResolver().openOutputStream(fileUri)) {
            if (outStream != null) {
                outStream.write(content.getBytes());
                outStream.close();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
