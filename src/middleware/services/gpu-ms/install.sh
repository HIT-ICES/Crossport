mkdir -p /opt/gpu-ms
cp *.py /opt/gpu-ms/
cp gpu-ms.service /etc/systemd/system/gpu-ms.service
systemctl daemon-reload
systemctl enable gpu-ms.service
systemctl start gpu-ms.service