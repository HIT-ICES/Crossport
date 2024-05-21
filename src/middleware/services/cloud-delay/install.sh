mkdir -p /opt/cloud-delay
cp *.sh /opt/cloud-delay/
cp cloud-delay.service /etc/systemd/system/cloud-delay.service
systemctl daemon-reload
systemctl enable cloud-delay.service
systemctl start cloud-delay.service
