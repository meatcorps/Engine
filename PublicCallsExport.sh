find . -name "*.cs" \
  -exec echo "==== {} ====" \; \
  -exec grep -E "public (class|struct|interface|record|readonly|static|sealed|virtual|override|[[:alnum:]_<>]+)" {} \; \
  > public_api.txt