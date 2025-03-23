import random
import concurrent.futures
from fileinput import filename
from itertools import product

import undetected_chromedriver as uc
import time
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import re
import threading
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options
from undetected_chromedriver import ChromeOptions
from concurrent.futures import ThreadPoolExecutor
import numpy as np

def get_options(is_headless=False):
    options = ChromeOptions()
    options.add_argument("--disable-gpu")
    options.add_argument("--no-sandbox")
    options.add_argument(f"--remote-debugging-port={9222 + threading.get_ident() % 4000}")
    if is_headless:
        options.add_argument("--headless")
    return options
def get_links(driver,stop_link:str,search_link:str,to_parse: int)->set:
    """Получает все ссылки на товары с DNS"""
    parsed_links = set()
    links =[]
    driver.get(f"{search_link}&page=1")
    time.sleep(5)
    page = driver.page_source
    soup = BeautifulSoup(page, 'html.parser')
    amount = int(soup.find('span',class_='count ng-star-inserted').text)//36

    amount = min(amount,to_parse)
    print(f'processing {amount} pages...')
    for x in range(1,amount+1):
        driver.get(f'{search_link}&page={x}')
        for i in range(7):
            driver.execute_script("window.scrollBy(0, 1900);")
            time.sleep(3)
            page = driver.page_source
            soup = BeautifulSoup(page, 'html.parser')
            raw_links = (soup.find_all('a', class_='product-title__text'))
            links+=(['https://www.mvideo.ru'+x.get('href')+'/specification' for x in raw_links])


    return set(links)

def get_product_description_mvid(desc_to_parse:str,source:str="NONE")->str:
    model = 'NONE'

    desc = ''

    soup = BeautifulSoup(desc_to_parse, 'html.parser')
    type = soup.find_all('li', class_='breadcrumbs__item ng-star-inserted')
    type = type[-2].text
    groups = soup.find_all('section', class_='characteristics__group ng-star-inserted')
    model = soup.find('span', class_='bar__product-title').text
    desc += 'Источник: ' + link + '\n'
    desc += 'Тип: ' + type + '\n'
    desc += 'Модель: ' + model + '\n'
    for group in groups:
        group_title = group.find('h2', class_='characteristics__group-title').text

        desc += group_title + ":" + '\n'

        key_vals = group.find_all('mvid-item-with-dots', class_='characteristics__list-item ng-star-inserted')
        for key_val in key_vals:
            key = key_val.find('dt', class_='item-with-dots__title').text
            val = key_val.find('dd', class_='item-with-dots__value').text
            desc += f'{key}: {val};'
        desc += '\n'
    return desc
def get_char_page (driver,link):
    driver.get(link)
    time.sleep(3)
    return driver.page_source

driver = uc.Chrome(options=get_options(is_headless=False))
link = 'https://www.mvideo.ru/stiralnye-i-sushilnye-mashiny-2427/stiralnye-mashiny-89?f_tolko-v-nalichii=da'
links = get_links(driver,'NONE',link,1)
link_amount=len(links)

print(link_amount)
print(links)

for i,link in enumerate(links):
    attrs = get_char_page(driver,link)
    chars = get_product_description_mvid(attrs,link)
    filename=link.split('/')[-2]+'.txt'
    with open(filename, 'w', encoding='utf-8') as f:
        f.write(chars)
    print(f'{link_amount -i-1} pages left.')