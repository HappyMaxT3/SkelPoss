import asyncio
import time

async def hello():
    print('Hello')
    # await asyncio.sleep(1)
    print('Hello')



async def main():
    await asyncio.gather(hello(), hello(), hello())


if __name__ == '__main__':
    asyncio.run(main())
