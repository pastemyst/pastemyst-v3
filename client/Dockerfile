FROM node:18-alpine

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build

ENTRYPOINT [ "node", "build/index.js" ]
