#!/bin/sh

if [ ! -e "/app/index_o.html" ]; then
    cp /app/index.html /app/index_o.html
fi
cp -f /app/index_o.html /app/index.html

# 输入文件名
input_file="/app/config.js"
# 输出文件名
output_file="/app/config_g.js"
if [ -f "${input_file}" ]; then
    awk '{
       while (match($0, /\$\{[A-Za-z_][A-Za-z0-9_]*\}/)) {
           var=substr($0, RSTART+2, RLENGTH-3)
           gsub(/\$\{[A-Za-z_][A-Za-z0-9_]*\}/, ENVIRON[var])
       }
       print
   }' "$input_file" >"$output_file"
fi

version=$(md5sum /app/config_g.js | awk '{print $1}')
version=${version:0:8}
cp -f /app/config_g.js "/app/config_${version}.js"
rm -f /app/config_g.js

sed -i "s#/config.js#${BASE_PATH}config_${version}.js#g" /app/index.html
sed -i "s#/assets/index-#${BASE_PATH}assets/index-#g" /app/index.html
