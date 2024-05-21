xvfb-run -a -s "-screen 0 2560x1440x24" \
    vglrun /opt/VirtualGL/bin/glxinfo | grep 'OpenGL'