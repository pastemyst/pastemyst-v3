FROM node:22-alpine

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build

EXPOSE 3000

ENTRYPOINT [ "node", "build" ]
