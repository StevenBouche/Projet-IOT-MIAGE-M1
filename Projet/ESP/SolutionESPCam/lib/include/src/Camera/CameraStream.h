#ifndef CameraStream_H
#define CameraStream_H

#include <Arduino.h>
#include "esp_camera.h"
#include "img_converters.h"
#include <functional>
#include "fb_gfx.h"
#include "fd_forward.h"
#include "fr_forward.h"

#define PWDN_GPIO_NUM     32
#define RESET_GPIO_NUM    -1
#define XCLK_GPIO_NUM      0
#define SIOD_GPIO_NUM     26
#define SIOC_GPIO_NUM     27

#define Y9_GPIO_NUM       35
#define Y8_GPIO_NUM       34
#define Y7_GPIO_NUM       39
#define Y6_GPIO_NUM       36
#define Y5_GPIO_NUM       21
#define Y4_GPIO_NUM       19
#define Y3_GPIO_NUM       18
#define Y2_GPIO_NUM        5
#define VSYNC_GPIO_NUM    25
#define HREF_GPIO_NUM     23
#define PCLK_GPIO_NUM     22

#define ENROLL_CONFIRM_TIMES 5
#define FACE_ID_SAVE_NUMBER 7

#define FACE_COLOR_WHITE  0x00FFFFFF
#define FACE_COLOR_BLACK  0x00000000
#define FACE_COLOR_RED    0x000000FF
#define FACE_COLOR_GREEN  0x0000FF00
#define FACE_COLOR_BLUE   0x00FF0000
#define FACE_COLOR_YELLOW (FACE_COLOR_RED | FACE_COLOR_GREEN)
#define FACE_COLOR_CYAN   (FACE_COLOR_BLUE | FACE_COLOR_GREEN)
#define FACE_COLOR_PURPLE (FACE_COLOR_BLUE | FACE_COLOR_RED)

typedef struct {
        size_t size; //number of values used for filtering
        size_t index; //current value index
        size_t count; //value count
        int sum;
        int * values; //array to be filled with values
} ra_filter_t;

typedef struct DataVideo {
  size_t size;
  const byte *buffer;
} DataVideo_t ;

class CameraStream : public TaskRobot {

    private:
        camera_config_t config;
        sensor_t * s;
        ra_filter_t ra_filter;
        mtmn_config_t mtmn_config;
        int ra_filter_run(ra_filter_t * filter, int value);
        void sendData(camera_fb_t * fb);

    protected:
    
        void execute();
        void task();
        void stop();

    public :

        QueueHandle_t queue;

        CameraStream() : TaskRobot("Camera", 32768, 0) { 
            queue = xQueueCreate(5, sizeof(struct DataVideo));
        }

        void init();
        

};

int CameraStream::ra_filter_run(ra_filter_t * filter, int value){
    if(!filter->values){
        return value;
    }
    filter->sum -= filter->values[filter->index];
    filter->values[filter->index] = value;
    filter->sum += filter->values[filter->index];
    filter->index++;
    filter->index = filter->index % filter->size;
    if (filter->count < filter->size) {
        filter->count++;
    }
    return filter->sum / filter->count;
}

void CameraStream::init(){

    config.ledc_channel = LEDC_CHANNEL_0;
    config.ledc_timer = LEDC_TIMER_0;
    config.pin_d0 = Y2_GPIO_NUM;
    config.pin_d1 = Y3_GPIO_NUM;
    config.pin_d2 = Y4_GPIO_NUM;
    config.pin_d3 = Y5_GPIO_NUM;
    config.pin_d4 = Y6_GPIO_NUM;
    config.pin_d5 = Y7_GPIO_NUM;
    config.pin_d6 = Y8_GPIO_NUM;
    config.pin_d7 = Y9_GPIO_NUM;
    config.pin_xclk = XCLK_GPIO_NUM;
    config.pin_pclk = PCLK_GPIO_NUM;
    config.pin_vsync = VSYNC_GPIO_NUM;
    config.pin_href = HREF_GPIO_NUM;
    config.pin_sscb_sda = SIOD_GPIO_NUM;
    config.pin_sscb_scl = SIOC_GPIO_NUM;
    config.pin_pwdn = PWDN_GPIO_NUM;
    config.pin_reset = RESET_GPIO_NUM;
    config.xclk_freq_hz = 20000000;
    config.pixel_format = PIXFORMAT_JPEG;

    // if PSRAM IC present, init with UXGA resolution and higher JPEG quality
    // for larger pre-allocated frame buffer.
    if(psramFound()){
        config.frame_size = FRAMESIZE_UXGA;
        config.jpeg_quality = 10;
        config.fb_count = 2;
    } else {
        config.frame_size = FRAMESIZE_SVGA;
        config.jpeg_quality = 12;
        config.fb_count = 1;
    }

    config.frame_size = FRAMESIZE_HVGA;
    config.jpeg_quality = 25;

    // camera init
    esp_err_t err = esp_camera_init(&config);
    if (err != ESP_OK) {
        Serial.printf("Camera init failed with error 0x%x", err);
        ESP.restart();
        return;
    }

    s = esp_camera_sensor_get();
    // initial sensors are flipped vertically and colors are a bit saturated

    Serial.print("PID Camera : ");
    Serial.println(s->id.PID);

    if (s->id.PID == OV3660_PID) {
        s->set_vflip(s, 1); // flip it back
        s->set_brightness(s, 1); // up the brightness just a bit
        s->set_saturation(s, -2); // lower the saturation
    }
}

void CameraStream::sendData(camera_fb_t * fb){
    DataVideo_t * data = reinterpret_cast<DataVideo*>(pvPortMalloc(sizeof(struct DataVideo)));
    data->size = fb->len;
    data->buffer = (const byte *)fb->buf;
    xQueueSend(queue, (void*) &data, portMAX_DELAY);
}

void CameraStream::task(){

    camera_fb_t * fb = NULL;
    dl_matrix3du_t *image_matrix = NULL;
    double waiting = 1000000/30;
    static int64_t current_frame = 0;
    static int64_t diff_frame = 0;
    static int64_t last_frame = 0;

    if(!last_frame) {
        last_frame = esp_timer_get_time();
    }

    Serial.print("Start read camera.");

    while(this->running){

        current_frame = esp_timer_get_time();
        diff_frame = current_frame - last_frame;

        if(diff_frame >= waiting){

            fb = esp_camera_fb_get();

            if(fb) {
                if(fb->width > 400) this->sendData(fb);
                else {
                    image_matrix = dl_matrix3du_alloc(1, fb->width, fb->height, 3);
                    if (image_matrix && fmt2rgb888(fb->buf, fb->len, fb->format, image_matrix->item)) 
                    {
                        this->sendData(fb);
                        dl_matrix3du_free(image_matrix);
                    }
                }
            }

            int64_t fr_end = esp_timer_get_time();
            int64_t frame_time = fr_end - last_frame;
            last_frame = fr_end;
            frame_time /= 1000;
            uint32_t avg_frame_time = ra_filter_run(&ra_filter, frame_time);

            /*Serial.printf("MJPG: %uB %ums (%.1ffps), AVG: %ums (%.1ffps)\n",
                (uint32_t)(fb->len),
                (uint32_t)frame_time, 
                1000.0 / (uint32_t)frame_time,
                avg_frame_time, 
                1000.0 / avg_frame_time
            );*/
        }
        else {
            vTaskDelay( (diff_frame/1000) / portTICK_PERIOD_MS);
        }

        if(fb){
            esp_camera_fb_return(fb);
            fb = NULL;
        } 
    }

    Serial.print("Stop task : Camera ");
}

void CameraStream::execute(){

    mtmn_config.type = FAST;
    mtmn_config.min_face = 80;
    mtmn_config.pyramid = 0.707;
    mtmn_config.pyramid_times = 4;
    mtmn_config.p_threshold.score = 0.6;
    mtmn_config.p_threshold.nms = 0.7;
    mtmn_config.p_threshold.candidate_number = 20;
    mtmn_config.r_threshold.score = 0.7;
    mtmn_config.r_threshold.nms = 0.7;
    mtmn_config.r_threshold.candidate_number = 10;
    mtmn_config.o_threshold.score = 0.7;
    mtmn_config.o_threshold.nms = 0.7;
    mtmn_config.o_threshold.candidate_number = 1;
    
}

void CameraStream::stop(){
    
}

#endif
