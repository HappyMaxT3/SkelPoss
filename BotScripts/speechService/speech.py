from transformers import (
    SpeechT5Processor,
    SpeechT5ForTextToSpeech,
    SpeechT5HifiGan
)
import torch
from datasets import load_dataset
import soundfile as sf
from IPython.display import Audio


processor = SpeechT5Processor.from_pretrained('microsoft/speecht5_tts')
model = SpeechT5ForTextToSpeech.from_pretrained('microsoft/speecht5_tts')
vocoder = SpeechT5HifiGan.from_pretrained('microsoft/speecht5_tts')

embeddings_dataset = load_dataset('Matthijs/cmu-arctic-xvectors', split='validation')
speaker_embeddings = torch.tensor(embeddings_dataset[7306]['xvector']).unsqueeze(0)

text = 'Привет! Давай пообщаемся! Как у тебя дела? Как тебя зовут? Сколько тебелет?'

inputs = processor(text=text, return_tensors='pt')
speech = model.generate_speech(inputs['input_ids'], speaker_embeddings=speaker_embeddings, vocoder=vocoder)

sf.write('output.wav', speech.numpy(), samplerate=22050)

Audio('output.wav')
