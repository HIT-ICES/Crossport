cp xhost-bypass.service /etc/systemd/system/xhost-bypass.service
systemctl daemon-reload
systemctl enable xhost-bypass.service
systemctl start xhost-bypass.service