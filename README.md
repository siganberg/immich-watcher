
## Environment 

| ENV Variables           | Required | Note                                                                                                                                                                          |  
|-------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|     IMMICH_INSTANCE_URL   | Yes      |  The hostname, IP address, or the DNS of your Immich server. <br>examples: <br>`https://immich.myserver.com` <br/> `http://192.168.1.2:2283`<br/>`http://myserver.local:2283` |
|     IMMICH_API_KEY    | Yes      | API Key. You can generate this on the from  `http://yourserver/user-settings?isOpen=api-keys`                                                                                 
                                    

## Volume mapping

| Path           | Required | Note                                                                                                                                                                                                                                                                                                                               |
|----------------|---------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `/var/lib/data` | Yes     | Host path where you want to watch `.jpg` and `.mp4` files . The service will automatically create 2 folders on this location on the first run. `pending` folder where you drop all files to be uploaded and `uploaded` folder where all the files that's has been uploaded will be move.<br/> Example: . `/mnt/user/immich-upload` |


## Sample docker-compose.

```yaml
name: immich-wather

services:
  server:
    image: ghcr.io/siganberg/immich-watcher:1.0.16
    container_name: immich_wather
    environment:
      - IMMICH_INSTANCE_URL=${IMMICH_INSTANCE_URL}
      - IMMICH_API_KEY=${IMMICH_API_KEY}
    restart: always
    volumes:
      - /mnt/user/immich-upload:/var/lib/data
```

