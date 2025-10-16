ENUM=SnakeFonts; \
ls -1 | grep -v '\.sh$' \
| sed 's/\.[^.]*$//' \
| sed 's/[-_]//g' \
| awk '{ out=""; for(i=1;i<=NF;i++){ w=$i; out=out toupper(substr(w,1,1)) substr(w,2) } print out }' \
| paste -sd, - \
| sed "s/^/enum ${ENUM} { /; s/$/ }/"