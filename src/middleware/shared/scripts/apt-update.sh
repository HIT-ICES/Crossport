cp /etc/apt/sources.list /etc/apt/sources.list.old
echo nameserver 192.168.1.1 >> /etc/resolv.conf
DEBIAN_FRONTEND=noninteractive

apt-get update
apt-get upgrade -y

apt-get install ca-certificates -y
apt-get install --no-install-recommends build-essential libc++1 apt-utils -y # 安装编译调试套件