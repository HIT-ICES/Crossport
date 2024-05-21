FROM 192.168.1.104:5000/metaverse/unity:2022.3.17-vgl3.1

COPY /app /app/main_Data
COPY *.cpcfg.json /app/

RUN ln -s /lib/x86_64-linux-gnu/libnvidia-encode.so.1 /lib/x86_64-linux-gnu/libnvidia-encode.so
RUN ln -s /lib/x86_64-linux-gnu/libnvcuvid.so.1 /lib/x86_64-linux-gnu/libnvcuvid.so
WORKDIR /app
