#!/bin/bash

npm run build-prod

cp dist/api.js /var/www/api/api_prod.js

pm2 restart api_prod